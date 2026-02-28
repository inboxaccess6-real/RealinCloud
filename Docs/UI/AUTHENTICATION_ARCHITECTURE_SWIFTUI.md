# Authentication Architecture - SwiftUI MVI Pattern

## 📐 Overview

This document describes the authentication implementation using the **Intent → Store → Reducer → Effect** pattern for the RealinAdmin iOS application built with SwiftUI.

## 🎯 Goals

1. **Single Source of Truth**: Auth state managed centrally in `AuthStore`
2. **Platform Native**: SwiftUI and iOS best practices with Keychain security
3. **Token Persistence**: JWT tokens stored securely with expiry handling
4. **State-Driven UI**: Login screen shown only when needed
5. **Reactive**: Leverages Combine for reactive state management
6. **Unidirectional Data Flow**: Intent → Store → Reducer → Effect

## 🏗️ Architecture Layers

### 1. **Models Layer** (`Features/Auth/Models/`)

Contains the MVI pattern definitions:

```
Features/Auth/Models/
├── AuthIntent.swift       # User actions/events
├── AuthState.swift        # UI state representation
└── AuthEffect.swift       # One-time side effects
```

### 2. **Store Layer** (`Features/Auth/Store/`)

State management with reducer pattern:

```
Features/Auth/Store/
├── AuthStore.swift        # State container with intent processing
└── AuthReducer.swift      # Pure reducer functions
```

### 3. **Data Layer** (`Features/Auth/Data/`)

Handles API calls and storage:

```
Features/Auth/Data/
├── Remote/
│   └── AuthService.swift      # Network API implementation
├── Local/
│   └── TokenStorage.swift     # Keychain token persistence
├── DTO/
│   └── AuthDTO.swift          # Request/Response models
└── AuthRepository.swift       # Repository pattern
```

### 4. **Views Layer** (`Features/Auth/Views/`)

SwiftUI components that observe state:

```
Features/Auth/Views/
├── AuthCoordinator.swift      # Root auth flow coordinator
├── LoginView.swift            # Email/Mobile input screen
└── OTPVerificationView.swift  # OTP input screen
```

## 🔄 Intent → Store → Reducer → Effect Flow

### 1. Intent (What can happen)

User actions that trigger state changes:

```swift
enum AuthIntent {
    case appStarted
    case sendOTP(method: LoginMethod, value: String)
    case verifyOTP(otp: String)
    case logout
    case retry
}
```

**Usage:**
```swift
store.send(.sendOTP(method: .email, value: "user@example.com"))
```

### 2. Store (Intent Processing)

Store receives intents and executes side effects:

```swift
@MainActor
class AuthStore: ObservableObject {
    @Published private(set) var state = AuthState()
    
    func send(_ intent: AuthIntent) {
        Task {
            await processIntent(intent)
        }
    }
    
    private func processIntent(_ intent: AuthIntent) async {
        switch intent {
        case .sendOTP(let method, let value):
            await handleSendOTP(method: method, value: value)
        // ... other intents
        }
    }
}
```

### 3. Reducer (State Updates)

Pure function that updates state based on results:

```swift
struct AuthReducer {
    static func reduce(state: inout AuthState, result: AuthResult) -> AuthEffect? {
        switch result {
        case .otpSent:
            state.isLoading = false
            state.status = .otpSent
            state.error = nil
            return .showSuccess(message: "OTP sent successfully")
        // ... other results
        }
    }
}
```

### 4. Effect (One-time Events)

Side effects emitted after state changes:

```swift
enum AuthEffect: Equatable {
    case navigateToHome
    case navigateToLogin
    case showError(message: String)
    case showSuccess(message: String)
}
```

## 📊 Data Flow Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│                         VIEW LAYER                                │
│  ┌────────────────┐         ┌────────────────┐                   │
│  │ AuthCoordinator│         │  LoginView     │                   │
│  │                │         │                │                   │
│  │ .onReceive()   │◄───────┤ @ObservedObject│                   │
│  │  (effects)     │         │    store       │                   │
│  └────────┬───────┘         └────────┬───────┘                   │
│           │                          │                            │
│  ┌────────▼──────────────────────────▼──────┐                    │
│  │          store.send(.intent)             │                    │
│  └──────────────────┬───────────────────────┘                    │
└─────────────────────┼────────────────────────────────────────────┘
                      │
                      ▼
┌──────────────────────────────────────────────────────────────────┐
│                      STORE LAYER                                  │
│  ┌──────────────────────────────────────────────────────┐        │
│  │               AuthStore                               │        │
│  │  func send(_ intent: AuthIntent)                     │        │
│  │     ↓                                                 │        │
│  │  processIntent() ← performs side effects             │        │
│  │     ↓                                                 │        │
│  │  applyResult() ← calls reducer                       │        │
│  └──────────┬────────────────────────┬───────────────────┘        │
│             │                        │                            │
│             │ result                 │ effect                     │
│             ▼                        ▼                            │
│  ┌──────────────────┐    ┌─────────────────────────┐            │
│  │  AuthReducer     │    │  effectSubject.send()   │            │
│  │  reduce()        │    │  (PassthroughSubject)   │            │
│  └──────────┬───────┘    └─────────────┬───────────┘            │
│             │                           │                         │
│             ▼                           │                         │
│  ┌──────────────────┐                  │                         │
│  │  @Published      │                  │                         │
│  │  state           │                  │                         │
│  └──────────┬───────┘                  │                         │
└─────────────┼──────────────────────────┼─────────────────────────┘
              │                          │
              │ state change             │ effect emitted
              ▼                          ▼
┌──────────────────────────────────────────────────────────────────┐
│                         VIEW LAYER                                │
│  ┌──────────────────────────────────────────────────┐            │
│  │  body re-renders                .onReceive()     │            │
│  │  (SwiftUI automatic)            handles effect   │            │
│  └──────────────────────────────────────────────────┘            │
└──────────────────────────────────────────────────────────────────┘
```

**Flow Summary:**
1. **View** sends intent → `store.send(.sendOTP)`
2. **Store** processes intent → performs API call
3. **Store** calls reducer → `applyResult(.otpSent)`
4. **Reducer** updates state → `state.status = .otpSent`
5. **Reducer** returns effect → `return .showSuccess(...)`
6. **Store** publishes state → `@Published state` updates
7. **Store** emits effect → `effectSubject.send(effect)`
8. **View** re-renders → SwiftUI observes `@Published state`
9. **Coordinator** handles effect → `.onReceive()` shows alert

## 🔐 Authentication Flow

### Flow Pattern

Every user interaction follows this pattern:

1. **Intent** - User action (button tap, app start)
2. **Store** - Processes intent, performs async operations
3. **Reducer** - Pure function updates state based on result
4. **Effect** - One-time event emitted (navigation, alert)
5. **View** - Re-renders based on new state, handles effects

### 1. App Startup

```
App Launch
    ↓
send(.appStarted)
    ↓
Store: Check Keychain tokens
    ↓
Reducer: reduce(.authenticationChecked)
    ↓
┌─────────────────┐
│ Token valid?    │
└────────┬────────┘
         │
    YES  │  NO / expired
         │
state.status = .authenticated    state.status = .unauthenticated
effect = .navigateToHome         effect = .navigateToLogin
```

**Implementation:**
```swift
// 1. Intent sent from view
store.send(.appStarted)

// 2. Store processes intent
private func processIntent(_ intent: AuthIntent) async {
    case .appStarted:
        await checkAuthentication()
}

// 3. Store performs side effect
private func checkAuthentication() async {
    state.isLoading = true
    
    do {
        let tokens = try tokenStorage.getTokens()
        if !tokens.isExpired {
            await applyResult(.authenticationChecked(isAuthenticated: true, tokens: tokens))
        } else {
            await applyResult(.authenticationChecked(isAuthenticated: false, tokens: nil))
        }
    } catch {
        await applyResult(.authenticationChecked(isAuthenticated: false, tokens: nil))
    }
}

// 4. Reducer updates state and returns effect
static func reduce(state: inout AuthState, result: AuthResult) -> AuthEffect? {
    case .authenticationChecked(let isAuthenticated, _):
        state.isLoading = false
        if isAuthenticated {
            state.status = .authenticated
            return .navigateToHome  // Effect emitted
        } else {
            state.status = .unauthenticated
            return .navigateToLogin  // Effect emitted
        }
}
```

### 2. Send OTP Flow

```
User enters email
    ↓
send(.sendOTP(method, value))
    ↓
Store: Call API
    ↓
Reducer: reduce(.otpSent)
    ↓
state.status = .otpSent
effect = .showSuccess
```

**Implementation:**
```swift
// 1. View sends intent
Button("Send OTP") {
    store.send(.sendOTP(method: .email, value: email))
}

// 2. Store processes and calls API
private func handleSendOTP(method: LoginMethod, value: String) async {
    state.isLoading = true
    state.contactInfo = value
    state.loginMethod = method
    
    do {
        let request = SendOTPRequest(method: method, value: value)
        _ = try await repository.sendOTP(request: request)
        await applyResult(.otpSent)
    } catch {
        await applyResult(.error(error))
    }
}

// 3. Reducer updates state
case .otpSent:
    state.isLoading = false
    state.status = .otpSent
    state.error = nil
    return .showSuccess(message: "OTP sent successfully")
```

### 3. Verify OTP Flow

```
User enters OTP
    ↓
send(.verifyOTP(otp))
    ↓
Store: Verify API + Save tokens
    ↓
Reducer: reduce(.otpVerified)
    ↓
state.status = .authenticated
effect = .navigateToHome
```

### 4. Token Expiry Handling

**Result Type:**
```swift
enum AuthResult {
    case authenticationChecked(isAuthenticated: Bool, tokens: TokenData?)
    case otpSent
    case otpVerified(tokens: TokenData)
    case tokenRefreshed(tokens: TokenData)
    case loggedOut
    case error(Error)
}
```

**On App Resume:**
```swift
.onAppear {
    store.send(.appStarted)  // Re-checks authentication
}
```

**Token Refresh Flow:**
```swift
private func refreshAccessToken(refreshToken: String) async {
    do {
        let response = try await repository.refreshToken(refreshToken: refreshToken)
        let tokens = TokenData(...)
        try tokenStorage.saveTokens(tokens)
        await applyResult(.tokenRefreshed(tokens: tokens))
    } catch {
        try? tokenStorage.clearTokens()
        await applyResult(.authenticationChecked(isAuthenticated: false, tokens: nil))
    }
}
```

## 🗄️ Token Storage

### iOS Implementation

Uses **Keychain** for secure token storage:

```swift
protocol TokenStorage {
    func saveTokens(_ tokens: TokenData) throws
    func getTokens() throws -> TokenData
    func clearTokens() throws
}

class KeychainTokenStorage: TokenStorage {
    private let service = "com.realin.admin"
    
    func saveTokens(_ tokens: TokenData) throws {
        // Saves to Keychain with kSecAttrAccessibleAfterFirstUnlock
    }
    
    func getTokens() throws -> TokenData {
        // Retrieves from Keychain
    }
    
    func clearTokens() throws {
        // Deletes from Keychain
    }
}
```

**Security Features:**
- Encrypted by iOS automatically
- Protected by device passcode/biometrics
- Survives app uninstall (unless explicitly cleared)
- Accessible after first unlock

## 🎨 UI Integration

### Root Coordinator

```swift
struct AuthCoordinator: View {
    @StateObject private var store = AuthStore()
    
    var body: some View {
        // UI is a pure function of state
        switch store.state.status {
        case .checking:
            SplashView()
            
        case .unauthenticated:
            LoginView(store: store)
            
        case .otpSent:
            OTPVerificationView(store: store)
            
        case .authenticated:
            MainAppView()
        }
    }
    
    .onAppear {
        setupEffectHandlers()
        store.send(.appStarted)  // Send intent on appear
    }
}
```

### View Integration

```swift
struct LoginView: View {
    @ObservedObject var store: AuthStore
    @State private var inputValue: String = ""
    
    var body: some View {
        VStack {
            // View observes state
            TextField("Email", text: $inputValue)
                .disabled(store.state.isLoading)
            
            // View sends intents
            Button("Send OTP") {
                store.send(.sendOTP(
                    method: .email,
                    value: inputValue
                ))
            }
            .disabled(!isInputValid || store.state.isLoading)
            
            // View displays state
            if let error = store.state.error {
                Text(error)
                    .foregroundColor(.red)
            }
        }
    }
}
```

### Effect Handling

Effects are **one-time events** that should be handled by the view layer. The coordinator subscribes to effects using SwiftUI's `.onReceive()` modifier:

```swift
struct AuthCoordinator: View {
    @StateObject private var store = AuthStore()
    @State private var showErrorAlert = false
    @State private var errorMessage = ""
    
    var body: some View {
        // ... your UI
        .onReceive(store.effects) { effect in
            handleEffect(effect)
        }
        .alert("Error", isPresented: $showErrorAlert) {
            Button("OK", role: .cancel) { }
        } message: {
            Text(errorMessage)
        }
    }
    
    private func handleEffect(_ effect: AuthEffect) {
        switch effect {
        case .showError(let message):
            errorMessage = message
            showErrorAlert = true
            
        case .showSuccess(let message):
            successMessage = message
            showSuccessAlert = true
            
        case .navigateToHome, .navigateToLogin:
            // Navigation handled by state change
            break
        }
    }
}
```

**Key Points:**
- ✅ Use `.onReceive()` instead of manual Combine subscription
- ✅ Effects are handled at the **coordinator level**, not in child views
- ✅ Child views only observe state via `@ObservedObject`
- ✅ This centralizes all side effect handling in one place

## 🧪 Testing Strategy

### Unit Tests - Reducer

Test pure reducer functions:

```swift
func testReducer_OTPSent() {
    var state = AuthState()
    
    let effect = AuthReducer.reduce(state: &state, result: .otpSent)
    
    XCTAssertEqual(state.status, .otpSent)
    XCTAssertEqual(state.isLoading, false)
    XCTAssertNil(state.error)
    XCTAssertEqual(effect, .showSuccess(message: "OTP sent successfully"))
}
```

### Unit Tests - Store

Test intent processing with mocks:

```swift
func testStore_SendOTP() async {
    let mockRepo = MockAuthRepository()
    let store = AuthStore(repository: mockRepo)
    
    store.send(.sendOTP(method: .email, value: "test@example.com"))
    
    await Task.sleep(nanoseconds: 100_000_000) // Wait for async
    
    XCTAssertEqual(store.state.status, .otpSent)
    XCTAssertEqual(store.state.contactInfo, "test@example.com")
}
```

### Integration Tests

- Full auth flow simulation
- Token expiry scenarios
- Network error handling
- Keychain operations

## 🚀 Benefits

1. **Unidirectional Data Flow**: Intent → Store → Reducer → Effect
2. **Testable**: Reducer is a pure function, Store uses dependency injection
3. **Predictable**: All state changes go through the reducer
4. **Type Safe**: Swift's type system ensures compile-time guarantees
5. **Separation of Concerns**: 
   - **Intents** define what can happen
   - **Store** handles side effects (API, storage)
   - **Reducer** updates state (pure function)
   - **Effects** handle one-time events
6. **Native**: Uses SwiftUI and iOS platform features (Keychain, Combine)
7. **Maintainable**: Clear separation makes code easy to understand and modify

## 🔄 State Transitions

```
.checking → .unauthenticated → .otpSent → .authenticated
    ↓                                          ↓
    └──────────────← .logout ←────────────────┘
```

**All transitions are unidirectional and predictable.**

## 📦 Dependencies

### Native iOS Frameworks

```swift
import Foundation      // Core Swift
import SwiftUI        // UI framework
import Combine        // Reactive programming
import Security       // Keychain access
```

No third-party dependencies required for core authentication.

## 🎓 Key Principles

1. **Unidirectional Flow**: Intent → Store → Reducer → Effect (never backwards)
2. **Single Source of Truth**: State lives only in the Store
3. **Pure Reducers**: Reducer functions have no side effects
4. **Side Effects in Store**: API calls, storage operations happen in Store
5. **State Drives UI**: Views are pure functions of state
6. **Effects for Events**: One-time events (navigation, alerts) use Effects
7. **Immutable State**: Reducer creates new state, never mutates directly
8. **Dependency Injection**: All dependencies injectable for testing

## 📐 Architecture Principles

### Intent
- Describes **what** the user wants to do
- Enum with associated values
- Sent from View to Store

### Store
- Receives intents
- Performs **side effects** (async operations)
- Calls reducer with results
- Emits effects

### Reducer
- **Pure function**: `(State, Result) -> (State, Effect?)`
- No side effects
- Deterministic state updates
- Returns optional effect

### Effect
- One-time events
- Handled by View
- Not stored in state
- Examples: navigation, alerts, analytics

## 🎬 How Effects Work

### Effect Flow Diagram

```
┌─────────────────────────────────────────────────────────┐
│  Store produces effect via PassthroughSubject           │
│  effectSubject.send(.showError("Invalid OTP"))          │
└────────────────────┬────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────┐
│  Coordinator subscribes via .onReceive(store.effects)   │
│  .onReceive(store.effects) { effect in ... }            │
└────────────────────┬────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────┐
│  Effect handler switches on effect type                 │
│  case .showError(let message):                          │
│      errorMessage = message                             │
│      showErrorAlert = true                              │
└─────────────────────────────────────────────────────────┘
```

### Why Effects Are Separate from State

**State** = Current condition (e.g., `isLoading`, `status`)
- Persists until changed
- Drives UI rendering
- Can be read multiple times

**Effect** = One-time event (e.g., "show alert", "navigate")
- Happens once
- Should not be stored in state
- Should not cause re-renders

### Example: Error Handling

**❌ Wrong - Storing effect in state:**
```swift
struct AuthState {
    var showErrorAlert: Bool = false  // ❌ This is an effect, not state!
    var errorMessage: String = ""
}
```
Problem: Alert shows again when view re-renders

**✅ Correct - Effect as one-time event:**
```swift
enum AuthEffect {
    case showError(message: String)  // ✅ One-time event
}

// In reducer
return .showError(message: error.localizedDescription)

// In coordinator
.onReceive(store.effects) { effect in
    case .showError(let message):
        showAlert = true  // Local @State in view
}
```

### Effect Subscription Pattern

**Only the root coordinator subscribes to effects:**

```swift
// ✅ AuthCoordinator - subscribes to effects
struct AuthCoordinator: View {
    @StateObject private var store = AuthStore()
    
    var body: some View {
        // ... UI based on state
        .onReceive(store.effects) { effect in
            handleEffect(effect)
        }
    }
}

// ✅ LoginView - only observes state
struct LoginView: View {
    @ObservedObject var store: AuthStore
    
    var body: some View {
        // Only reads store.state, never subscribes to effects
        TextField("Email", text: $email)
            .disabled(store.state.isLoading)
    }
}
```

**Why only coordinator?**
- Centralizes effect handling
- Prevents duplicate alerts
- Easier to maintain
- Clear separation of concerns

### Effect vs State Decision Tree

```
Is this value used to render UI?
    ├─ YES → State (e.g., isLoading, username)
    └─ NO
        ├─ Does it happen once? → Effect (e.g., showAlert, navigate)
        └─ Does it persist? → State (e.g., error message displayed in UI)
```

## 🔧 Configuration

### Update API Base URL

Edit `AuthService.swift`:

```swift
private init(
    baseURL: String = "https://api.realin.com", // Your API URL
    session: URLSession = .shared
) {
    self.baseURL = baseURL
    self.session = session
}
```

### Customize Token Expiry

Edit `TokenData` in `AuthDTO.swift`:

```swift
var willExpireSoon: Bool {
    let bufferTime = Date().addingTimeInterval(300) // 5 minutes
    return bufferTime >= expiresAt
}
```

## 📱 Usage in App

### Setup in App Entry Point

```swift
@main
struct RealinAdminApp: App {
    var body: some Scene {
        WindowGroup {
            AuthCoordinator()
        }
    }
}
```

### Access Auth State in Other Views

```swift
struct SomeView: View {
    @EnvironmentObject var authStore: AuthStore
    
    var body: some View {
        Button("Logout") {
            authStore.dispatch(.logout)
        }
    }
}
```

## 🔍 Debugging

### Enable Logging

Add to `AuthStore`:

```swift
private func handle(_ intent: AuthIntent) async {
    print("🔵 Intent: \(intent)")
    // ... handle intent
    print("🟢 New State: \(state)")
}
```

### Monitor Effects

```swift
effectSubject.send(.showError(message: error))
print("🔴 Effect: showError(\(error))")
```

## 📊 File Structure Summary

```
Features/Auth/
├── Models/
│   ├── AuthIntent.swift        # User actions (enum)
│   ├── AuthState.swift         # UI state (struct)
│   └── AuthEffect.swift        # Side effects (enum)
├── Store/
│   ├── AuthStore.swift         # @MainActor, ObservableObject
│   │                           # - Processes intents
│   │                           # - Performs side effects
│   │                           # - Applies reducer
│   └── AuthReducer.swift       # Pure reducer (struct)
│                               # - reduce(state, result) -> effect?
├── Data/
│   ├── Remote/
│   │   └── AuthService.swift   # URLSession API calls
│   ├── Local/
│   │   └── TokenStorage.swift  # Keychain operations
│   ├── DTO/
│   │   └── AuthDTO.swift       # Codable models + AuthResult
│   └── AuthRepository.swift    # Repository pattern
└── Views/
    ├── AuthCoordinator.swift   # Root coordinator
    ├── LoginView.swift         # Login UI
    └── OTPVerificationView.swift # OTP UI
```

## 🎯 Best Practices

1. **Always send intents** - Use `store.send(.intent)`, never modify state directly
2. **Keep reducers pure** - No side effects in reducer functions
3. **Side effects in Store** - API calls, storage, timers belong in Store
4. **Use @ObservedObject** - Let SwiftUI handle re-rendering automatically
5. **Handle effects** - Set up effect handlers in `.onAppear`
6. **Inject dependencies** - Pass store and services for testability
7. **Use async/await** - Modern Swift concurrency in Store
8. **Secure tokens** - Always use Keychain, never UserDefaults
9. **Validate input** - Check email/mobile format before sending intent
10. **Clear error messages** - Use localized, user-friendly error descriptions

## 🔄 Complete Example Flow

```swift
// 1. User taps button
Button("Send OTP") {
    store.send(.sendOTP(method: .email, value: "user@example.com"))
}

// 2. Store processes intent
func send(_ intent: AuthIntent) {
    Task {
        await processIntent(intent)
    }
}

private func processIntent(_ intent: AuthIntent) async {
    case .sendOTP(let method, let value):
        await handleSendOTP(method: method, value: value)
}

// 3. Store performs side effect (API call)
private func handleSendOTP(method: LoginMethod, value: String) async {
    state.isLoading = true
    state.contactInfo = value
    state.loginMethod = method
    
    do {
        _ = try await repository.sendOTP(...)
        await applyResult(.otpSent)  // ← Success result
    } catch {
        await applyResult(.error(error))  // ← Error result
    }
}

// 4. Store applies result through reducer
private func applyResult(_ result: AuthResult) async {
    if let effect = AuthReducer.reduce(state: &state, result: result) {
        effectSubject.send(effect)
    }
}

// 5. Reducer updates state and returns effect
static func reduce(state: inout AuthState, result: AuthResult) -> AuthEffect? {
    case .otpSent:
        state.isLoading = false      // ← State update
        state.status = .otpSent      // ← State update
        state.error = nil            // ← State update
        return .showSuccess(...)     // ← Effect returned
}

// 6. View re-renders based on new state
var body: some View {
    if store.state.status == .otpSent {
        OTPVerificationView(store: store)  // ← UI updates
    }
}

// 7. Effect handler shows success message
.sink { effect in
    case .showSuccess(let message):
        showAlert = true  // ← Effect handled
}
```

---

**Last Updated**: December 18, 2025  
**Version**: 1.0  
**Platform**: iOS (SwiftUI)
