# Authentication Architecture - MVI Pattern

## 📐 Overview

This document describes the authentication implementation using the MVI (Model-View-Intent) pattern for the RealinAdmin KMP application.

## 🎯 Goals

1. **Single Source of Truth**: Auth state managed centrally
2. **Platform Agnostic**: Works seamlessly on Android, iOS, WASM, Desktop
3. **Token Persistence**: JWT tokens stored securely with expiry handling
4. **State-Driven UI**: Login screen shown only when needed
5. **WASM-Friendly**: Handles page reloads and token expiry gracefully

## 🏗️ Architecture Layers

### 1. **Domain Layer** (`domain/auth/`)

Contains business logic and models:

```
domain/auth/
├── models/
│   ├── AuthIntent.kt       # User actions/events
│   ├── AuthState.kt        # UI state
│   ├── AuthEffect.kt       # One-time side effects
│   └── AuthResult.kt       # Internal results
├── AuthStore.kt            # MVI store (state container)
└── AuthRepository.kt       # Data operations
```

### 2. **Data Layer** (`data/auth/`)

Handles API calls and storage:

```
data/auth/
├── remote/
│   ├── AuthApi.kt          # API interface
│   └── dto/                # Request/Response models
├── local/
│   └── TokenStore.kt       # Token persistence
└── AuthRepositoryImpl.kt   # Repository implementation
```

### 3. **Presentation Layer** (`ui/screens/auth/`)

UI components that observe state:

```
ui/screens/auth/
├── LoginScreen.kt          # Email/Mobile input
├── OtpVerificationScreen.kt # OTP input
└── components/             # Reusable UI components
```

## 🔄 MVI Flow

### Intent (What can happen)

```kotlin
sealed interface AuthIntent {
    object AppStarted : AuthIntent
    data class SendOtp(val method: LoginMethod, val value: String) : AuthIntent
    data class VerifyOtp(val otp: String) : AuthIntent
    object Logout : AuthIntent
}
```

### State (Current UI state)

```kotlin
data class AuthState(
    val status: AuthStatus = AuthStatus.Checking,
    val contactInfo: String = "",
    val loginMethod: LoginMethod? = null,
    val isLoading: Boolean = false,
    val error: String? = null
)

sealed class AuthStatus {
    object Checking : AuthStatus()
    object Unauthenticated : AuthStatus()
    object OtpSent : AuthStatus()
    object Authenticated : AuthStatus()
}
```

### Effect (One-time events)

```kotlin
sealed interface AuthEffect {
    object NavigateToHome : AuthEffect
    object NavigateToLogin : AuthEffect
    data class ShowError(val message: String) : AuthEffect
}
```

## 🔐 Authentication Flow

### 1. App Startup

```
App Start
    ↓
dispatch(AppStarted)
    ↓
Check stored tokens
    ↓
┌─────────────────┐
│ Token valid?    │
└────────┬────────┘
         │
    YES  │  NO / expired
         │
    Main App    Login Screen
```

**Implementation:**
```kotlin
private fun checkAuth() {
    scope.launch {
        val tokens = tokenStore.getTokens()
        
        if (tokens == null || tokens.isExpired()) {
            _state.update { it.copy(status = AuthStatus.Unauthenticated) }
            _effects.emit(AuthEffect.NavigateToLogin)
        } else {
            _state.update { it.copy(status = AuthStatus.Authenticated) }
            _effects.emit(AuthEffect.NavigateToHome)
        }
    }
}
```

### 2. OTP Request Flow

```
User enters email/mobile
    ↓
dispatch(SendOtp(method, value))
    ↓
Call API: POST /auth/send-otp
    ↓
Success → status = OtpSent
    ↓
Show OTP input screen
```

### 3. OTP Verification Flow

```
User enters OTP
    ↓
dispatch(VerifyOtp(otp))
    ↓
Call API: POST /auth/verify-otp
    ↓
Receive JWT tokens
    ↓
Save to TokenStore
    ↓
status = Authenticated
    ↓
emit(NavigateToHome)
```

### 4. Token Expiry Handling

**On Every API Call:**
```kotlin
// HTTP Interceptor
if (accessToken.isExpired()) {
    val newTokens = refreshAccessToken()
    if (newTokens != null) {
        tokenStore.save(newTokens)
        retry(request)
    } else {
        dispatch(Logout)
    }
}
```

**On App Resume (WASM page reload):**
```kotlin
// Re-check auth on app start
dispatch(AppStarted)
```

## 🗄️ Token Storage

### Android/iOS
- Uses `multiplatform-settings` library
- Encrypted SharedPreferences (Android)
- Keychain (iOS)

### WASM
- Uses `localStorage`
- Base64 encoded (HTTPS ensures transport security)

### Desktop
- Uses platform-specific preferences

```kotlin
interface TokenStore {
    suspend fun save(accessToken: String, refreshToken: String, expiresAt: Long)
    suspend fun getTokens(): TokenData?
    suspend fun clear()
}
```

## 🎨 UI Integration

### Root Composable

```kotlin
@Composable
fun AppRoot(authStore: AuthStore) {
    val state by authStore.state.collectAsState()
    
    LaunchedEffect(Unit) {
        authStore.dispatch(AuthIntent.AppStarted)
    }
    
    when (state.status) {
        AuthStatus.Checking -> SplashScreen()
        AuthStatus.Unauthenticated -> LoginScreen(authStore)
        AuthStatus.OtpSent -> OtpVerificationScreen(authStore)
        AuthStatus.Authenticated -> MainApp()
    }
}
```

### Screen Integration

```kotlin
@Composable
fun LoginScreen(authStore: AuthStore) {
    val state by authStore.state.collectAsState()
    
    // UI renders based on state
    OutlinedTextField(
        value = emailOrMobile,
        enabled = !state.isLoading,
        isError = state.error != null
    )
    
    Button(
        onClick = {
            authStore.dispatch(
                AuthIntent.SendOtp(method, value)
            )
        },
        enabled = !state.isLoading
    )
}
```

## 🧪 Testing Strategy

### Unit Tests
- **Reducer Tests**: Pure function testing
- **Store Tests**: Intent → State transitions
- **Repository Tests**: Mocked API responses

### Integration Tests
- **Flow Tests**: Full auth flow simulation
- **Token Expiry**: Refresh token scenarios
- **Error Handling**: Network failures, invalid OTP

## 🚀 Benefits

1. **No Navigation Hacks**: UI is pure function of state
2. **Testable**: Each component is independently testable
3. **Predictable**: State changes are deterministic
4. **Platform Agnostic**: Same logic across all platforms
5. **WASM Optimized**: Handles page reloads seamlessly
6. **Type Safe**: Compile-time guarantees

## 🔄 State Transitions

```
Checking → Unauthenticated → OtpSent → Authenticated
    ↓                                        ↓
    └──────────────← Logout ←───────────────┘
```

## 📦 Dependencies

```kotlin
commonMain.dependencies {
    implementation("com.russhwolf:multiplatform-settings:1.1.1")
    implementation("io.ktor:ktor-client-core:2.3.7")
    implementation("io.ktor:ktor-client-content-negotiation:2.3.7")
    implementation("io.ktor:ktor-serialization-kotlinx-json:2.3.7")
    implementation("org.jetbrains.kotlinx:kotlinx-coroutines-core:1.7.3")
}
```

## 🎓 Key Principles

1. **Auth is Global State**: Not navigation state
2. **UI Never Reads Tokens**: Always through AuthStore
3. **Login Screen is Conditional**: Based on AuthStatus
4. **Effects for Navigation**: Not stored in state
5. **State Decides Rendering**: Pure reactive approach

---

**Last Updated**: December 17, 2025  
**Version**: 1.0
