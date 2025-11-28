//
//  OTPServiceProtocol.swift
//  RealinApi
//
//  Created by Murali Krishna Garapati on 22/11/25.
//

protocol OTPServiceProtocol {
    func generateOTP(to destination: String, channel: OTPChannel) async throws -> 
    
}
