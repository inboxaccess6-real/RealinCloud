import AsyncHTTPClient
import Foundation
import NIOCore
import NIOHTTP1

/// Google OAuth token info response structure
struct GoogleTokenInfo: Codable {
    let sub: String  // Google user ID
    let email: String
    let emailVerified: String?
    let name: String?
    let picture: String?
    let givenName: String?
    let familyName: String?
    let locale: String?

    enum CodingKeys: String, CodingKey {
        case sub
        case email
        case emailVerified = "email_verified"
        case name
        case picture
        case givenName = "given_name"
        case familyName = "family_name"
        case locale
    }
}

/// Service for verifying Google OAuth tokens
actor GoogleOAuthService : OAuthServiceProtocol {
    private let httpClient: HTTPClient
    private let tokenInfoEndpoint = "https://oauth2.googleapis.com/tokeninfo"

    init(httpClient: HTTPClient) {
        self.httpClient = httpClient
    }

    /// Verify a Google ID token and return user information
    /// - Parameter idToken: The Google ID token from the frontend
    /// - Returns: GoogleTokenInfo containing user details
    /// - Throws: Error if token is invalid or verification fails
    func verifyToken(_ idToken: String) async throws -> OAuthUserInfo {
        // Build the request URL with the token as a query parameter
        guard var urlComponents = URLComponents(string: tokenInfoEndpoint) else {
            throw OAuthError.invalidURL
        }

        urlComponents.queryItems = [URLQueryItem(name: "id_token", value: idToken)]

        guard let url = urlComponents.url?.absoluteString else {
            throw OAuthError.invalidURL
        }

        // Create and execute the HTTP request
        var request = HTTPClientRequest(url: url)
        request.method = .GET

        let response = try await httpClient.execute(request, timeout: .seconds(30))

        // Check if the response status is OK
        guard response.status == .ok else {
            let bodyBytes = try await response.body.collect(upTo: 1024 * 1024)  // 1MB limit
            let bodyString = String(buffer: bodyBytes)
            throw OAuthError.invalidToken(reason: "Google verification failed: \(bodyString)")
        }

        // Parse the response body
        let bodyBytes = try await response.body.collect(upTo: 1024 * 1024)
        let decoder = JSONDecoder()
        let tokenInfo = try decoder.decode(GoogleTokenInfo.self, from: bodyBytes)

        // Verify email is verified
        if tokenInfo.emailVerified != "true" {
            throw OAuthError.emailNotVerified
        }

        return OAuthUserInfo(userId: tokenInfo.sub, email: tokenInfo.email, name: tokenInfo.name ?? "", picture: tokenInfo.picture ?? "")
    }
}

/// Common OAuth errors
enum OAuthError: Error, CustomStringConvertible {
    case invalidURL
    case invalidToken(reason: String)
    case emailNotVerified
    case networkError(String)
    case decodingError(String)

    var description: String {
        switch self {
        case .invalidURL:
            return "Invalid OAuth endpoint URL"
        case .invalidToken(let reason):
            return "Invalid token: \(reason)"
        case .emailNotVerified:
            return "Email address is not verified"
        case .networkError(let message):
            return "Network error: \(message)"
        case .decodingError(let message):
            return "Failed to decode response: \(message)"
        }
    }
}
