import Foundation

/// Mock SMS service for development
/// In production, replace with actual SMS service (Twilio, AWS SNS, etc.)
actor MockSMSService: OTPChannelSender {
    func send(to phoneNumber: String, code: String) async throws {
        // In development, just log the OTP
        print("📱 [MOCK SMS] Sending OTP to \(phoneNumber)")
        print("🔑 OTP Code: \(code)")
        print("⏰ Valid for: 10 minutes")
        print("---")
        
        // Simulate network delay
        try await Task.sleep(for: .milliseconds(100))
    }
}
// MARK: - Production SMS Service Examples

/// Example Twilio SMS service
/// Uncomment and implement when ready for production
/*
import AsyncHTTPClient

actor TwilioSMSService: SMSServiceProtocol {
    private let accountSid: String
    private let authToken: String
    private let fromPhoneNumber: String
    private let httpClient: HTTPClient
    
    init(
        accountSid: String,
        authToken: String,
        fromPhoneNumber: String,
        httpClient: HTTPClient
    ) {
        self.accountSid = accountSid
        self.authToken = authToken
        self.fromPhoneNumber = fromPhoneNumber
        self.httpClient = httpClient
    }
    
    func sendOTP(to phoneNumber: String, code: String) async throws {
        let message = "Your verification code is: \(code). Valid for 10 minutes."
            </style>
        </head>
        <body>
            <div class="container">
                <h2>Your Verification Code</h2>
                <p>Use the following code to complete your sign-in:</p>
                <div class="code-box">\(code)</div>
                <p>This code will expire in 10 minutes.</p>
                <p>If you didn't request this code, please ignore this email.</p>
            </div>
        </body>
        </html>
        """
        
        // Implement SMTP sending logic here
        // This is just a template - actual implementation depends on the SMTP library
    }
}
*/

/// Example SendGrid email service
/*
import AsyncHTTPClient

actor SendGridEmailService: EmailServiceProtocol {
    private let apiKey: String
        
        // Build URL-encoded form data
        var urlComponents = URLComponents()
        urlComponents.queryItems = [
            URLQueryItem(name: "To", value: phoneNumber),
            URLQueryItem(name: "From", value: fromPhoneNumber),
            URLQueryItem(name: "Body", value: message)
        ]
        
        guard let formData = urlComponents.query?.data(using: .utf8) else {
            throw OTPError.smsSendFailed
        }
        
        // Create request
        let url = "https://api.twilio.com/2010-04-01/Accounts/\(accountSid)/Messages.json"
        var request = HTTPClientRequest(url: url)
        request.method = .POST
        
        // Basic auth
        let credentials = "\(accountSid):\(authToken)"
        if let credentialsData = credentials.data(using: .utf8) {
            let base64Credentials = credentialsData.base64EncodedString()
            request.headers.add(name: "Authorization", value: "Basic \(base64Credentials)")
        }
        
        request.headers.add(name: "Content-Type", value: "application/x-www-form-urlencoded")
        request.body = .bytes(ByteBuffer(data: formData))
        
        let response = try await httpClient.execute(request, timeout: .seconds(30))
        
        guard response.status == .created || response.status == .ok else {
            throw OTPError.smsSendFailed
        }
    }
}
*/

/// Example AWS SNS SMS service
/*
import SotoSNS

actor AWSSNSSMSService: SMSServiceProtocol {
    private let snsClient: SNS
    
    init(snsClient: SNS) {
        self.snsClient = snsClient
    }
    
    func sendOTP(to phoneNumber: String, code: String) async throws {
        let message = "Your verification code is: \(code). Valid for 10 minutes."
        
        let request = SNS.PublishInput(
            message: message,
            phoneNumber: phoneNumber
        )
        
        do {
            _ = try await snsClient.publish(request)
        } catch {
            throw OTPError.smsSendFailed
        }
    }
}
*/
