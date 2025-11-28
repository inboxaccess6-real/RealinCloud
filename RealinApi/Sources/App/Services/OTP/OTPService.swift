import Foundation
import Fluent

/// Service for managing OTP generation and verification
actor OTPService {
    // OTP expiration: 10 minutes
    private let otpExpirationTime: TimeInterval
    
    // Maximum verification attempts
    private let maxAttempts: Int
    
    // OTP code length
    private let codeLength: Int
    
    init(
        otpExpirationTime: TimeInterval = 10 * 60, // 10 minutes
        maxAttempts: Int = 5,
        codeLength: Int = 6
    ) {
        self.otpExpirationTime = otpExpirationTime
        self.maxAttempts = maxAttempts
        self.codeLength = codeLength
    }
    
    /// Generate and store a new OTP for a phone number
    /// - Parameters:
    ///   - phoneNumber: The phone number (E.164 format recommended)
    ///   - ipAddress: Optional IP address of requester
    ///   - db: Database connection
    /// - Returns: The generated OTP code
    func generateOTP(
        phoneNumber: String,
        ipAddress: String? = nil,
        on db: Database
    ) async throws -> String {
        // Invalidate any existing OTPs for this phone number
        try await invalidateExistingOTPs(phoneNumber: phoneNumber, on: db)
        
        // Generate a random 6-digit code
        let code = generateCode()
        
        // Calculate expiration
        let expiresAt = Date().addingTimeInterval(otpExpirationTime)
        
        // Create OTP record
        let otp = OTP(
            phoneNumber: phoneNumber,
            code: code,
            expiresAt: expiresAt,
            ipAddress: ipAddress
        )
        
        try await otp.save(on: db)
        
        return code
    }
    
    /// Verify an OTP code for a phone number
    /// - Parameters:
    ///   - phoneNumber: The phone number
    ///   - code: The OTP code to verify
    ///   - db: Database connection
    /// - Returns: True if valid
    /// - Throws: OTPError if invalid, expired, or max attempts exceeded
    func verifyOTP(
        phoneNumber: String,
        code: String,
        on db: Database
    ) async throws -> Bool {
        // Find the most recent unused OTP for this phone number
        guard let otp = try await OTP.query(on: db)
            .filter(\.$phoneNumber == phoneNumber)
            .filter(\.$isUsed == false)
            .sort(\.$createdAt, .descending)
            .first() else {
            throw OTPError.invalidOTP
        }
        
        // Check if expired
        guard otp.expiresAt > Date() else {
            throw OTPError.otpExpired
        }
        
        // Check max attempts
        guard otp.attempts < maxAttempts else {
            throw OTPError.maxAttemptsExceeded
        }
        
        // Increment attempts
        otp.attempts += 1
        try await otp.save(on: db)
        
        // Verify code
        guard otp.code == code else {
            if otp.attempts >= maxAttempts {
                throw OTPError.maxAttemptsExceeded
            }
            throw OTPError.invalidOTP
        }
        
        // Mark as used
        otp.isUsed = true
        try await otp.save(on: db)
        
        return true
    }
    
    /// Invalidate all existing OTPs for a phone number
    private func invalidateExistingOTPs(phoneNumber: String, on db: Database) async throws {
        try await OTP.query(on: db)
            .filter(\.$phoneNumber == phoneNumber)
            .filter(\.$isUsed == false)
            .set(\.$isUsed, to: true)
            .update()
    }
    
    /// Delete expired OTPs (cleanup task)
    func deleteExpiredOTPs(on db: Database) async throws {
        try await OTP.query(on: db)
            .filter(\.$expiresAt < Date())
            .delete()
    }
    
    /// Generate a random numeric code
    private func generateCode() -> String {
        let digits = (0..<codeLength).map { _ in Int.random(in: 0...9) }
        return digits.map { String($0) }.joined()
    }
    
    /// Check rate limiting for OTP requests
    /// - Parameters:
    ///   - phoneNumber: The phone number
    ///   - db: Database connection
    /// - Returns: True if allowed, false if rate limited
    func checkRateLimit(phoneNumber: String, on db: Database) async throws -> Bool {
        // Count OTPs created in the last hour for this phone number
        let oneHourAgo = Date().addingTimeInterval(-3600)
        
        let count = try await OTP.query(on: db)
            .filter(\.$phoneNumber == phoneNumber)
            .filter(\.$createdAt >= oneHourAgo)
            .count()
        
        // Allow maximum 5 OTP requests per hour
        return count < 5
    }
}

/// OTP errors
enum OTPError: Error, CustomStringConvertible {
    case invalidOTP
    case otpExpired
    case maxAttemptsExceeded
    case rateLimitExceeded
    case smsSendFailed
    
    var description: String {
        switch self {
        case .invalidOTP:
            return "Invalid OTP code"
        case .otpExpired:
            return "OTP has expired"
        case .maxAttemptsExceeded:
            return "Maximum verification attempts exceeded"
        case .rateLimitExceeded:
            return "Too many OTP requests. Please try again later"
        case .smsSendFailed:
            return "Failed to send OTP SMS"
        }
    }
}
