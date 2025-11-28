//
//  OAuthServiceProtocol.swift
//  RealinApi
//
//  Created by Murali Krishna Garapati on 22/11/25.
//

protocol OAuthServiceProtocol {
    func verifyToken(_ token: String) async throws -> OAuthUserInfo
}
