//
//  OTPStore.swift
//  RealinApi
//
//  Created by Murali Krishna Garapati on 22/11/25.
//
import Foundation
import Fluent

struct OTPStore : OTPStoreProtocol {
    func invalidateAll(for recipient: String, on db: Database) async throws
    {
        try await OTP.query(on: db)
            .filter(\.$recipient == recipient)
            .filter(\.$isUsed == false)
            .set(\.$isUsed, to: true)
            .update()
    }
    
    func create(recipient: String, code: String, expiresAt: Date, ipAddress: String?, on db: Database) async throws
    {
        // Create OTP record
        let otp = OTP(
            recipient: recipient,
            code: code,
            expiresAt: expiresAt,
            ipAddress: ipAddress
        )
        
        try await otp.save(on: db)
    }
    
    func findLatestValid(for recipient: String, on db: Database) async throws -> OTP?
    {
        // Find the most recent unused, unexpired OTP for this recipient
        return try await OTP.query(on: db)
            .filter(\.$recipient == recipient)
            .filter(\.$isUsed == false)
            .filter(\.$expiresAt > Date())
            .sort(\.$createdAt, .descending)
            .first()
    }
    
    func incrementAttempts(_ otp: OTP, on db: Database) async throws
    {
        // Increment attempts
        otp.attempts += 1
        try await otp.save(on: db)
    }
    
    func markUsed(_ otp: OTP, on db: Database) async throws
    {
        otp.isUsed = true
        try await otp.save(on: db)
    }
    
    func checkRateLimit(for recipient: String, timeWindow: TimeInterval, maximumRequests:Int, on db: Database) async throws -> Bool
    {
        // Count OTPs created in the last time window for this recipient ex:- last 1 hour if timeWindow is 3600
        let windowStart = Date().addingTimeInterval(-timeWindow)
        
        let count = try await OTP.query(on: db)
            .filter(\.$recipient == recipient)
            .filter(\.$createdAt >= windowStart)
            .count()
        
        // Allow maximum OTP requests per hour ex:- 5 OTPs per 1 hour
        return count < maximumRequests
    }
    
    /// Delete expired OTPs (cleanup task)
    func deleteExpired(on db: Database) async throws {
        try await OTP.query(on: db)
            .filter(\.$expiresAt < Date())
            .delete()
    }
}
