import Hummingbird
import Foundation

/// Extended request context that includes authenticated user information
protocol AuthenticatedRequestContext: RequestContext {
    var userId: UUID? { get set }
}

/// Default implementation of authenticated request context
struct AuthenticatedContext: AuthenticatedRequestContext {
    var coreContext: CoreRequestContextStorage
    var userId: UUID?
    
    init(source: Source) {
        self.coreContext = .init(source: source)
        self.userId = nil
    }
}

/// Middleware that extracts user ID from JWT and stores it in context
struct JWTAuthenticationMiddleware: RouterMiddleware {
    let jwtService: JWTService
    
    func handle(
        _ request: Request,
        context: AuthenticatedContext,
        next: (Request, AuthenticatedContext) async throws -> Response
    ) async throws -> Response {
        // Extract the Authorization header
        guard let authHeader = request.headers[.authorization] else {
            return Response(
                status: .unauthorized,
                headers: [.contentType: "application/json"],
                body: .init(byteBuffer: try encodeError("Missing authorization header"))
            )
        }
        
        // Extract bearer token (authHeader is String in Hummingbird 2.0)
        guard authHeader.hasPrefix("Bearer ") else {
            return Response(
                status: .unauthorized,
                headers: [.contentType: "application/json"],
                body: .init(byteBuffer: try encodeError("Invalid authorization format. Use 'Bearer <token>'"))
            )
        }
        let token = String(authHeader.dropFirst("Bearer ".count))
        
        // Verify the token
        do {
            let payload = try await jwtService.verifyToken(token)
            
            // Create new context with user ID
            var authenticatedContext = context
            authenticatedContext.userId = payload.userId
            
            // Continue to next middleware/handler
            return try await next(request, authenticatedContext)
        } catch {
            return Response(
                status: .unauthorized,
                headers: [.contentType: "application/json"],
                body: .init(byteBuffer: try encodeError("Invalid or expired token"))
            )
        }
    }
    
    private func encodeError(_ message: String) throws -> ByteBuffer {
        let errorResponse = ["error": message]
        let data = try JSONEncoder().encode(errorResponse)
        var buffer = ByteBuffer()
        buffer.writeBytes(data)
        return buffer
    }
}
