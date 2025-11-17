import Hummingbird
import HummingbirdFluent
import Fluent
import FluentPostgresDriver
import AsyncHTTPClient
import Logging
import Foundation

/// Application arguments protocol. We use a protocol so we can call
/// `buildApplication` inside Tests as well as in the App executable. 
/// Any variables added here also have to be added to `App` in App.swift and 
/// `TestArguments` in AppTest.swift
package protocol AppArguments {
    var hostname: String { get }
    var port: Int { get }
    var logLevel: Logger.Level? { get }
    var databaseUrl: String? { get }
    var jwtSecret: String? { get }
    var appleBundleId: String? { get }
}

// Request context used by application - supports authenticated user context
typealias AppRequestContext = AuthenticatedContext

///  Build application
/// - Parameter arguments: application arguments
func buildApplication(_ arguments: some AppArguments) async throws -> some ApplicationProtocol {
    let environment = Environment()
    let logger = {
        var logger = Logger(label: "RealinApi")
        logger.logLevel = 
            arguments.logLevel ??
            environment.get("LOG_LEVEL").flatMap { Logger.Level(rawValue: $0) } ??
            .info
        return logger
    }()
    
    // Get configuration from environment or arguments
    let databaseUrl = arguments.databaseUrl ?? 
        environment.get("DATABASE_URL") ?? 
        "postgres://localhost/realin_db"
    
    let jwtSecret = arguments.jwtSecret ?? 
        environment.get("JWT_SECRET") ?? 
        "change-this-secret-in-production"
    
    let appleBundleId = arguments.appleBundleId ?? 
        environment.get("APPLE_BUNDLE_ID") ?? 
        "com.example.realin"
    
    let autoMigrate: Bool = environment.get("AUTO_MIGRATE")?.lowercased() == "true"

    // Configure Fluent database
    let fluent = try await configureFluent(databaseUrl: databaseUrl, logger: logger)
    
    // Create HTTP client for OAuth services
    let httpClient = HTTPClient(eventLoopGroupProvider: .singleton)
    
    // Initialize services
    let jwtService = try JWTService(secret: jwtSecret) // 30 minutes for access token
    let refreshTokenService = RefreshTokenService() // 90 days for refresh token
    let googleOAuthService = GoogleOAuthService(httpClient: httpClient)
    let appleOAuthService = AppleOAuthService(httpClient: httpClient, appBundleId: appleBundleId)
    let otpService = OTPService() // OTP service for phone authentication
    let smsService = ConsoleSMSService() // Console SMS service for development
    
    // Build router with services
    let router = try await buildRouter(
        fluent: fluent,
        jwtService: jwtService,
        refreshTokenService: refreshTokenService,
        googleOAuthService: googleOAuthService,
        appleOAuthService: appleOAuthService,
        otpService: otpService,
        smsService: smsService
    )
    
    let app = Application(
        router: router,
        configuration: .init(
            address: .hostname(arguments.hostname, port: arguments.port),
            serverName: "RealinApi"
        ),
        logger: logger
    )
    return app
}

/// Configure Fluent with PostgreSQL and run migrations
func configureFluent(databaseUrl: String, logger: Logger) async throws -> Fluent {
    // Parse PostgreSQL URL
    guard let url = URL(string: databaseUrl),
          let host = url.host else {
        throw FluentError.invalidDatabaseURL
    }
    
    let port = url.port ?? 5432
    let username = url.user ?? "postgres"
    let password = url.password ?? ""
    let database = url.path.trimmingCharacters(in: CharacterSet(charactersIn: "/"))
    
    // Configure database
    let databaseConfig = SQLPostgresConfiguration(
        hostname: host,
        port: port,
        username: username,
        password: password,
        database: database.isEmpty ? "postgres" : database,
        tls: .disable
    )
    
    // Create Fluent instance
    let fluent = Fluent(logger: logger)
    
    // Add PostgreSQL database
    fluent.databases.use(
        .postgres(configuration: databaseConfig),
        as: DatabaseID(string: "psql")
    )
    
    // Register migrations
    await fluent.migrations.add(CreateUser())
    await fluent.migrations.add(CreateRefreshToken())
    await fluent.migrations.add(CreateOTP())
    
    // Run migrations automatically
    try await fluent.migrate()
    
    logger.info("Database migrations completed successfully")
    
    return fluent
}

/// Build router
func buildRouter(
    fluent: Fluent,
    jwtService: JWTService,
    refreshTokenService: RefreshTokenService,
    googleOAuthService: GoogleOAuthService,
    appleOAuthService: AppleOAuthService,
    otpService: OTPService,
    smsService: SMSServiceProtocol
) async throws -> Router<AppRequestContext> {
    let router = Router(context: AppRequestContext.self)
    
    // Add middleware
    router.addMiddleware {
        // logging middleware
        LogRequestsMiddleware(.info)
    }
    
    // Add default endpoint
    router.get("/") { _,_ in
        return "RealinApi - OAuth Authentication Ready!"
    }
    
    // Health check endpoint
    router.get("/health") { _, _ in
        return ["status": "ok", "timestamp": ISO8601DateFormatter().string(from: Date())]
    }
    
    // Add authentication routes
    let authController = AuthController(
        googleOAuthService: googleOAuthService,
        appleOAuthService: appleOAuthService,
        jwtService: jwtService,
        refreshTokenService: refreshTokenService,
        otpService: otpService,
        smsService: smsService,
        fluent: fluent
    )
    authController.addRoutes(to: router)
    
    return router
}

/// Custom error for Fluent configuration
enum FluentError: Error {
    case invalidDatabaseURL
}

/// Environment helper
struct Environment {
    func get(_ key: String) -> String? {
        return ProcessInfo.processInfo.environment[key]
    }
}
