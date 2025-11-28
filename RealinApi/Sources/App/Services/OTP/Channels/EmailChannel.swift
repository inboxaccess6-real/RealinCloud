//
//  EmailService.swift
//  RealinApi
//
//  Created by Murali Krishna Garapati on 23/11/25.
//

actor MockEmailSender: OTPChannelSender {
    func send(to recipient: String, code: String) async throws {
        
    }
}
