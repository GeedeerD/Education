# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Structure

This is a mixed repository containing:
- **`FiweClient/`** — the main Avalonia cross-platform messenger app (desktop + Android)
- **`FiweClient.Crypto/`** — E2EE cryptography library used by FiweClient
- **`FiweClient.Droid/`** — Android entry-point project (thin wrapper over FiweClient.App)
- Various educational/exercise projects (`Enums/`, `DZ11/`, `sea-battle/`, etc.) — standalone C# console apps

The primary active project is **FiweClient**, governed by `FiweClient/FiweClient.sln`.

## Build & Run Commands

```bash
# Build the FiweClient solution (desktop + crypto)
dotnet build FiweClient/FiweClient.sln

# Run the desktop app
dotnet run --project FiweClient/FiweClient.App.csproj

# Build for Android (produces .apk via FiweClient.Droid)
dotnet build FiweClient.Droid/FiweClient.Droid.csproj -f net8.0-android

# Build a specific educational project
dotnet build <ProjectFolder>/<Project>.csproj
```

There are no automated tests in this repository.

## FiweClient Architecture

### Technology Stack
- **UI**: Avalonia 11.1 with Fluent theme, compiled bindings (`AvaloniaUseCompiledBindingsByDefault`)
- **MVVM**: CommunityToolkit.Mvvm (`[ObservableProperty]`, `[RelayCommand]`, `ObservableObject`)
- **DI**: `Microsoft.Extensions.DependencyInjection` — all services registered in `App.axaml.cs → ConfigureServices`
- **Realtime**: SignalR (`Microsoft.AspNetCore.SignalR.Client`) connecting to `/chatHub`
- **Crypto**: BouncyCastle (X25519 ECDH + AES-256-GCM), wrapped in `FiweClient.Crypto`
- **API base URL**: `https://fiwe-api-dgabh9axgnhagqhg.italynorth-01.azurewebsites.net/`

### Project Split: Desktop vs Android
`FiweClient.App.csproj` targets `net8.0` (desktop) and `net8.0-android`. `Program.cs` is compiled only for `net8.0`. `FiweClient.Droid` is the Android-specific executable entry point that references `FiweClient.App`.

In `App.OnFrameworkInitializationCompleted`:
- `IClassicDesktopStyleApplicationLifetime` → creates `MainWindow` (desktop)
- `ISingleViewApplicationLifetime` → creates `MainView` (Android)

Platform-conditional code uses `#if ANDROID` to switch file storage paths between `%AppData%/Fiwe/` and Android's `FilesDir`.

### Navigation Pattern
`INavigationService` sets `MainWindowViewModel.CurrentView` (an `ObservableObject`). Avalonia's `DataTemplate` system in AXAML automatically resolves the correct `View` for each ViewModel type. Navigation is always done through `INavigationService.NavigateTo<TViewModel>()` or `.NavigateTo(viewModel)`.

Two-level navigation:
1. **Top-level**: `NavigationService` swaps the root view (Login → Shell)
2. **In-shell**: `ShellViewModel.CurrentContent` swaps the right-pane content (Chats / Contacts / Settings / Chat)

### Service Lifetimes (registered in `ConfigureServices`)
| Lifetime | Services |
|----------|----------|
| Singleton | `SessionService`, `TokenStorage`, `SignalRService`, `NavigationService`, `CryptoService`, `KeyStorageService`, `SharedSecretCache`, `ShellViewModel`, `ChatListViewModel`, `ContactListViewModel`, `MainWindowViewModel` |
| Transient | `AuthApiService`, `MessageApiService`, `ContactApiService`, `UserApiService`, `LoginViewModel`, `RegisterViewModel`, `ChatViewModel`, `AddContactViewModel`, `SettingsViewModel` |

### Authentication & Session Flow
1. On startup, `TryAutoLoginAsync` loads `session.json` from disk via `ITokenStorage`
2. If the JWT is still valid (`exp > now + 1min`), session is restored and SignalR reconnects
3. On login, after JWT is received, `LoginViewModel` parses `userId` from the `nameid`/`nameidentifier` claim
4. All API requests inherit from `BaseApiService`, which injects `Authorization: Bearer <token>` automatically

### E2EE Cryptography
The encryption scheme (all in `FiweClient.Crypto`):
- **Key generation**: X25519 key pair generated once per device per user
- **Key storage**: Private key encrypted with AES-256-GCM (key derived via PBKDF2-SHA256, 100k iterations) and saved to `{userId}.dat`; public key published to the server
- **Shared secret**: `ECDH(myPrivate, contactPublic)` → SHA-256 hash → used as AES key. Cached in `ISharedSecretCache` for the session duration
- **Message encryption**: AES-256-GCM (BouncyCastle), format: `[12-byte nonce][ciphertext + 16-byte GCM tag]`, base64-encoded
- The server stores and transmits only encrypted ciphertext; plaintext never leaves the device

### SignalR Event Flow
Server fires `ReceiveMessage(chatId, recipients[], messageId)`. `ChatViewModel` receives it via `IRealtimeService.MessageReceived` event, checks if `session.UserId` is in recipients, fetches the encrypted message body by `messageId`, decrypts, and posts to `Dispatcher.UIThread`.
