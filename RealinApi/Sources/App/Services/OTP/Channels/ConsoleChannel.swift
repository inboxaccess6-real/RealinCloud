//
//  ConsoleService.swift
//  RealinApi
//
//  Created by Murali Krishna Garapati on 23/11/25.
//

/// Console SMS service for development (prints to console)
actor ConsoleSMSService: OTPServiceProtocol {
    func sendOTP(to phoneNumber: String, code: String) async throws {
        let message = """
        
        ================================================================================
        📱 SMS SERVICE (Console)
        ================================================================================
        To: \(phoneNumber)
        Message:
        
        Your verification code is: \(code)
        
        This code will expire in 10 minutes.
        
        If you didn't request this code, please ignore this message.
        ================================================================================
        
        """
        print(message)
    }
}
