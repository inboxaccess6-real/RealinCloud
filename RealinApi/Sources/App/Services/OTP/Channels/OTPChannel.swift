import Foundation

enum OTPChannel: String, Codable {
    case sms
    case email
    case console
}

protocol OTPChannelSender {
    func send(to recipient: String, code: String) async throws
}

protocol OTPChannelFactoryProtocol {
    func make(for channel: OTPChannel) -> OTPChannelSender
}

struct OTPChannelFactory: OTPChannelFactoryProtocol {
    let sms: MockSMSService?
    let email: MockEmailService
    let console: MockConsoleService
    
    func make(for channel: OTPChannel) -> OTPChannelSender {
        switch channel {
        case .sms:
            return sms
        case .email:
            return email
        case .console:
            return console
        }
    }
}
