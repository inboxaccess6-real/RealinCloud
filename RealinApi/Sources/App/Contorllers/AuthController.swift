import Hummingbird
import HummingbirdFluent
import Fluent
import Foundation
import AsyncHTTPClient

/// Request/Response DTOs for OAuth endpoints

struct GoogleLoginRequest: Codable {
    let idToken: String
}

struct AppleLoginRequest: Codable {
    let idToken: String
    let name: String? // Apple provides name only on first sign-in
}

struct LoginResponse: Codable {
    let accessToken: String
    let refreshToken: String
    let user: UserResponse
    let expiresIn: Int // Access token expiration in seconds
}

struct RefreshTokenRequest: Codable {
    let refreshToken: String
}

struct RefreshTokenResponse: Codable {
    let accessToken: String
    let refreshToken: String
    let expiresIn: Int
}

struct SendOTPRequest: Codable {
    let phoneNumber: String
}

struct SendOTPResponse: Codable {
    let message: String
    let expiresIn: Int
}

struct VerifyOTPRequest: Codable {
    let phoneNumber: String
    let code: String
    let name: String? // Optional name for first-time users
}

struct ErrorResponse: Codable {
    let error: String
}

/// Controller handling OAuth authentication endpoints
struct AuthController {
    let googleOAuthService: GoogleOAuthService
    let appleOAuthService: AppleOAuthService
    let jwtService: JWTService
    let refreshTokenService: RefreshTokenService
    let otpService: OTPService
    let smsService: SMSServiceProtocol
    let fluent: Fluent
    
    /// Add auth routes to the router
    func addRoutes(to router: Router<AuthenticatedContext>) {
        let db = fluent.db()
        
        // Public routes (no authentication required)
        router.post("/auth/google/login") { request, context in
            try await self.googleLogin(request, context: context, db: db)
        }
        router.post("/auth/apple/login") { request, context in
            try await self.appleLogin(request, context: context, db: db)
        }
        router.post("/auth/refresh") { request, context in
            try await self.refreshAccessToken(request, context: context, db: db)
        }
        
        // OTP authentication routes
        router.post("/auth/otp/send") { request, context in
            try await self.sendOTP(request, context: context, db: db)
        }
        router.post("/auth/otp/verify") { request, context in
            try await self.verifyOTP(request, context: context, db: db)
        }
        
        // Protected routes (authentication required) - use middleware for security
        let protectedRouter = router.group()
            .add(middleware: JWTAuthenticationMiddleware(jwtService: jwtService))
        
        protectedRouter.get("/auth/me") { request, context in
            try await self.getCurrentUser(request, context: context, db: db)
        }
        protectedRouter.post("/auth/logout") { request, context in
            try await self.logout(request, context: context, db: db)
        }
    }
    
    /// Handle Google OAuth login
    @Sendable
    func googleLogin(
        _ request: Request,
        context: some RequestContext,
        db: Database
    ) async throws -> Response {
        // Decode the request body
        let body = try await request.body.collect(upTo: .max)
        
        let loginRequest: GoogleLoginRequest
        do {
            loginRequest = try JSONDecoder().decode(GoogleLoginRequest.self, from: body)
        } catch {
            return try errorResponse(status: .badRequest, message: "Invalid request format")
        }
        
        // Verify the Google token
        let tokenInfo: GoogleTokenInfo
        do {
            tokenInfo = try await googleOAuthService.verifyToken(loginRequest.idToken)
        } catch {
            return try errorResponse(status: .unauthorized, message: "Invalid Google token: \(error)")
        }
        
        // Find or create user
        let user: User
        if let existingUser = try await User.query(on: db)
            .filter(\.$oauthProvider == .google)
            .filter(\.$oauthId == tokenInfo.sub)
            .first() {
            // Update existing user info if changed
            existingUser.email = tokenInfo.email
            existingUser.name = tokenInfo.name
            existingUser.pictureUrl = tokenInfo.picture
            try await existingUser.save(on: db)
            user = existingUser
        } else {
            // Create new user
            user = User(
                oauthProvider: .google,
                oauthId: tokenInfo.sub,
                email: tokenInfo.email,
                name: tokenInfo.name,
                pictureUrl: tokenInfo.picture
            )
            try await user.save(on: db)
        }
        
        // Generate JWT token
        guard let userId = user.id else {
            return try errorResponse(status: .internalServerError, message: "User ID not available")
        }
        
        let jwtToken = try await jwtService.generateToken(userId: userId, email: user.email ?? "")
        
        // Generate refresh token
        let refreshToken = try await refreshTokenService.generateRefreshToken(
            userId: userId,
            deviceName: request.headers[.userAgent],
            ipAddress: extractIPAddress(from: request),
            on: db
        )
        
        // Create response
        let response = LoginResponse(
            accessToken: jwtToken,
            refreshToken: refreshToken,
            user: try UserResponse(from: user),
            expiresIn: 1800 // 30 minutes
        )
        
        return try jsonResponse(status: .ok, body: response)
    }
    
    /// Handle Apple OAuth login
    @Sendable
    func appleLogin(
        _ request: Request,
        context: some RequestContext,
        db: Database
    ) async throws -> Response {
        // Decode the request body
        let body = try await request.body.collect(upTo: .max)
        
        let loginRequest: AppleLoginRequest
        do {
            loginRequest = try JSONDecoder().decode(AppleLoginRequest.self, from: body)
        } catch {
            return try errorResponse(status: .badRequest, message: "Invalid request format")
        }
        
        // Verify the Apple token
        let tokenClaims: AppleIDTokenClaims
        do {
            tokenClaims = try await appleOAuthService.verifyToken(loginRequest.idToken)
        } catch {
            return try errorResponse(status: .unauthorized, message: "Invalid Apple token: \(error)")
        }
        
        // Find or create user
        let user: User
        if let existingUser = try await User.query(on: db)
            .filter(\.$oauthProvider == .apple)
            .filter(\.$oauthId == tokenClaims.sub.value)
            .first() {
            // Update existing user info if changed
            if let email = tokenClaims.email {
                existingUser.email = email
            }
            if let name = loginRequest.name {
                existingUser.name = name
            }
            try await existingUser.save(on: db)
            user = existingUser
        } else {
            // Create new user
            guard let email = tokenClaims.email else {
                return try errorResponse(status: .badRequest, message: "Email not provided by Apple")
            }
            
            user = User(
                oauthProvider: .apple,
                oauthId: tokenClaims.sub.value,
                email: email,
                name: loginRequest.name
            )
            try await user.save(on: db)
        }
        
        // Generate JWT token
        guard let userId = user.id else {
            return try errorResponse(status: .internalServerError, message: "User ID not available")
        }
        
        let jwtToken = try await jwtService.generateToken(userId: userId, email: user.email ?? "")
        
        // Generate refresh token
        let refreshToken = try await refreshTokenService.generateRefreshToken(
            userId: userId,
            deviceName: request.headers[.userAgent],
            ipAddress: extractIPAddress(from: request),
            on: db
        )
        
        // Create response
        let response = LoginResponse(
            accessToken: jwtToken,
            refreshToken: refreshToken,
            user: try UserResponse(from: user),
            expiresIn: 1800 // 30 minutes
        )
        
        return try jsonResponse(status: .ok, body: response)
    }
    
    /// Refresh access token using refresh token
    @Sendable
    func refreshAccessToken(
        _ request: Request,
        context: some RequestContext,
        db: Database
    ) async throws -> Response {
        // Decode the request body
        let body = try await request.body.collect(upTo: .max)
        
        let refreshRequest: RefreshTokenRequest
        do {
            refreshRequest = try JSONDecoder().decode(RefreshTokenRequest.self, from: body)
        } catch {
            return try errorResponse(status: .badRequest, message: "Invalid request format")
        }
        
        // Verify refresh token
        let userId: UUID
        do {
            userId = try await refreshTokenService.verifyRefreshToken(refreshRequest.refreshToken, on: db)
        } catch {
            return try errorResponse(status: .unauthorized, message: "Invalid or expired refresh token")
        }
        
        // Get user from database
        guard let user = try await User.find(userId, on: db) else {
            return try errorResponse(status: .notFound, message: "User not found")
        }
        
        // Generate new access token
        let newAccessToken = try await jwtService.generateToken(userId: userId, email: user.email ?? "")
        
        // Generate new refresh token and revoke the old one
        try await refreshTokenService.revokeRefreshToken(refreshRequest.refreshToken, on: db)
        let newRefreshToken = try await refreshTokenService.generateRefreshToken(
            userId: userId,
            deviceName: request.headers[.userAgent],
            ipAddress: extractIPAddress(from: request),
            on: db
        )
        
        // Create response
        let response = RefreshTokenResponse(
            accessToken: newAccessToken,
            refreshToken: newRefreshToken,
            expiresIn: 1800 // 30 minutes
        )
        
        return try jsonResponse(status: .ok, body: response)
    }
    
    /// Logout user by revoking refresh token
    @Sendable
    func logout(
        _ request: Request,
        context: some AuthenticatedRequestContext,
        db: Database
    ) async throws -> Response {
        // JWT verification and userId extraction done by JWTAuthenticationMiddleware
        guard let userId = context.userId else {
            return try errorResponse(status: .unauthorized, message: "User not authenticated")
        }
        
        // Revoke all refresh tokens for this user
        try await refreshTokenService.revokeAllUserTokens(userId: userId, on: db)
        
        return try jsonResponse(status: .ok, body: ["message": "Logged out successfully"])
    }
    
    /// Send OTP to phone number
    @Sendable
    func sendOTP(
        _ request: Request,
        context: some RequestContext,
        db: Database
    ) async throws -> Response {
        // Decode the request body
        let body = try await request.body.collect(upTo: .max)
        
        let otpRequest: SendOTPRequest
        do {
            otpRequest = try JSONDecoder().decode(SendOTPRequest.self, from: body)
        } catch {
            return try errorResponse(status: .badRequest, message: "Invalid request format")
        }
        
        // Validate phone number format (basic validation)
        guard isValidPhoneNumber(otpRequest.phoneNumber) else {
            return try errorResponse(status: .badRequest, message: "Invalid phone number format")
        }
        
        // Check rate limiting
        let isAllowed = try await otpService.checkRateLimit(phoneNumber: otpRequest.phoneNumber, on: db)
        guard isAllowed else {
            return try errorResponse(status: .tooManyRequests, message: "Too many OTP requests. Please try again later")
        }
        
        // Generate OTP
        let ipAddress = extractIPAddress(from: request)
        let code = try await otpService.generateOTP(
            phoneNumber: otpRequest.phoneNumber,
            ipAddress: ipAddress,
            on: db
        )
        
        // Send OTP via SMS
        do {
            try await smsService.sendOTP(to: otpRequest.phoneNumber, code: code)
        } catch {
            return try errorResponse(status: .internalServerError, message: "Failed to send OTP SMS")
        }
        
        // Return success response
        let response = SendOTPResponse(
            message: "OTP sent successfully to \(otpRequest.phoneNumber)",
            expiresIn: 600 // 10 minutes
        )
        
        return try jsonResponse(status: .ok, body: response)
    }
    
    /// Verify OTP and login/register user
    @Sendable
    func verifyOTP(
        _ request: Request,
        context: some RequestContext,
        db: Database
    ) async throws -> Response {
        // Decode the request body
        let body = try await request.body.collect(upTo: .max)
        
        let verifyRequest: VerifyOTPRequest
        do {
            verifyRequest = try JSONDecoder().decode(VerifyOTPRequest.self, from: body)
        } catch {
            return try errorResponse(status: .badRequest, message: "Invalid request format")
        }
        
        // Verify OTP
        let isValid: Bool
        do {
            isValid = try await otpService.verifyOTP(
                phoneNumber: verifyRequest.phoneNumber,
                code: verifyRequest.code,
                on: db
            )
        } catch let error as OTPError {
            return try errorResponse(status: .unauthorized, message: error.description)
        } catch {
            return try errorResponse(status: .unauthorized, message: "Invalid OTP code")
        }
        
        guard isValid else {
            return try errorResponse(status: .unauthorized, message: "Invalid OTP code")
        }
        
        // Find or create user
        let user: User
        if let existingUser = try await User.query(on: db)
            .filter(\.$phoneNumber == verifyRequest.phoneNumber)
            .first() {
            // Update existing user info if name provided
            if let name = verifyRequest.name {
                existingUser.name = name
                try await existingUser.save(on: db)
            }
            user = existingUser
        } else {
            // Create new user with phone authentication
            user = User(
                phoneNumber: verifyRequest.phoneNumber,
                name: verifyRequest.name
            )
            try await user.save(on: db)
        }
        
        // Generate JWT token
        guard let userId = user.id else {
            return try errorResponse(status: .internalServerError, message: "User ID not available")
        }
        
        // Use phone number as email fallback for JWT (or use a unique identifier)
        let jwtToken = try await jwtService.generateToken(userId: userId, email: user.email ?? user.phoneNumber ?? "")
        
        // Generate refresh token
        let refreshToken = try await refreshTokenService.generateRefreshToken(
            userId: userId,
            deviceName: request.headers[.userAgent],
            ipAddress: extractIPAddress(from: request),
            on: db
        )
        
        // Create response
        let response = LoginResponse(
            accessToken: jwtToken,
            refreshToken: refreshToken,
            user: try UserResponse(from: user),
            expiresIn: 1800 // 30 minutes
        )
        
        return try jsonResponse(status: .ok, body: response)
    }
    
    /// Get current authenticated user
    @Sendable
    func getCurrentUser(
        _ request: Request,
        context: some AuthenticatedRequestContext,
        db: Database
    ) async throws -> Response {

        // No token extraction needed!
        guard let userId = context.userId else {
            return try errorResponse(status: .unauthorized, message: "User not authenticated")
        }
        
        // Fetch user from database
        guard let user = try await User.find(userId, on: db) else {
            return try errorResponse(status: .notFound, message: "User not found")
        }
        
        // Return user information
        let userResponse = try UserResponse(from: user)
        return try jsonResponse(status: .ok, body: userResponse)
    }
    
    // Helper methods for creating responses
    private func jsonResponse<T: Encodable>(status: HTTPResponse.Status, body: T) throws -> Response {
        let data = try JSONEncoder().encode(body)
        var buffer = ByteBuffer()
        buffer.writeBytes(data)
        
        return Response(
            status: status,
            headers: [.contentType: "application/json"],
            body: .init(byteBuffer: buffer)
        )
    }
    
    private func errorResponse(status: HTTPResponse.Status, message: String) throws -> Response {
        return try jsonResponse(status: status, body: ErrorResponse(error: message))
    }
    
    /// Validate phone number format (basic validation for E.164 format)
    /// E.164 format: +[country code][subscriber number] (e.g., +14155552671)
    private func isValidPhoneNumber(_ phoneNumber: String) -> Bool {
        // Basic validation: should start with + and contain 10-15 digits
        let phoneRegex = "^\\+[1-9]\\d{9,14}$"
        let phonePredicate = NSPredicate(format: "SELF MATCHES %@", phoneRegex)
        return phonePredicate.evaluate(with: phoneNumber)
    }
    
    /// Validate email format
    private func isValidEmail(_ email: String) -> Bool {
        let emailRegex = "^[A-Z0-9a-z._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$"
        let emailPredicate = NSPredicate(format: "SELF MATCHES %@", emailRegex)
        return emailPredicate.evaluate(with: email)
    }
    
    /// Extract IP address from request
    private func extractIPAddress(from request: Request) -> String? {
        // In Hummingbird 2.0, headers API changed and we would need to access
        // the raw socket for real IP. For now, return nil as it's optional.
        // TODO: Implement proper IP extraction when Hummingbird provides access
        return nil
    }
}
