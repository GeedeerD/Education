# FiweClient — Документация проекта

## Обзор

**FiweClient** — кроссплатформенный мессенджер с end-to-end шифрованием (E2EE), написанный на C# (.NET 8) с использованием Avalonia UI. Приложение работает на Windows/Linux/macOS и Android.

Репозиторий содержит три основных проекта:

| Проект | Назначение |
|--------|-----------|
| `FiweClient/` (`FiweClient.App.csproj`) | Основное приложение: UI, ViewModels, сервисы |
| `FiweClient.Crypto/` | Библиотека криптографии (X25519 + AES-256-GCM) |
| `FiweClient.Droid/` | Android-точка входа (тонкая обёртка над `FiweClient.App`) |

Решение: `FiweClient/FiweClient.sln`

---

## Технологический стек

| Слой | Технология |
|------|-----------|
| UI-фреймворк | Avalonia 11.1, Fluent-тема, скомпилированные байндинги |
| MVVM | CommunityToolkit.Mvvm (`[ObservableProperty]`, `[RelayCommand]`) |
| DI-контейнер | `Microsoft.Extensions.DependencyInjection` |
| Realtime-связь | SignalR (`Microsoft.AspNetCore.SignalR.Client`) → `/chatHub` |
| Криптография | BouncyCastle (X25519 ECDH, AES-256-GCM) |
| API | REST HTTP через `IHttpClientFactory`, base URL: Azure Italy North |

---

## Архитектура

### Паттерны и принципы

- **MVVM** — каждый экран имеет пару `View.axaml` + `ViewModel.cs`
- **DI + интерфейсы** — все сервисы регистрируются в `App.axaml.cs → ConfigureServices`; зависимости передаются через конструкторы
- **NavigationService** — устанавливает `MainWindowViewModel.CurrentView`; Avalonia находит нужный `View` через `DataTemplate` в AXAML автоматически
- **Платформенный код** — `#if ANDROID` разделяет хранилище файлов (`%AppData%/Fiwe/` vs `FilesDir`) и реализацию QR-сканера

### Жизненный цикл при старте

```
App.OnFrameworkInitializationCompleted
 ├── ConfigureServices()          — строим DI-контейнер
 ├── Создаём MainWindow / MainView (зависит от платформы)
 └── TryAutoLoginAsync()
      ├── appSettings.LoadAsync()  — загружаем настройки (UTC-offset)
      ├── tokenStorage.LoadAsync() — читаем session.json с диска
      │
      ├─[токен валиден]──────────────────────────────────────────────
      │   ├─ [нет ключей на устройстве] → KeyTransferView (перенос ключей)
      │   └─ [ключи есть] → SignalR.ConnectAsync() → ShellView
      │
      └─[токен невалиден / отсутствует] → LoginView
```

---

## Навигация

Два уровня:

1. **Корневой** (`NavigationService` → `MainWindowViewModel.CurrentView`):
   - `LoginView` → `RegisterView` → `ShellView` (или `KeyTransferView`)

2. **Внутри Shell** (`ShellViewModel.CurrentContent`):
   - Chats (`ChatListView` / `ChatView`)
   - Contacts (`ContactListView` / `AddContactView`)
   - Settings (`SettingsView`)

---

## Слои приложения

### ViewModels

| ViewModel | Lifetime | Ответственность |
|-----------|----------|----------------|
| `MainWindowViewModel` | Singleton | Хранит `CurrentView` — текущий активный экран |
| `ShellViewModel` | Singleton | Shell-оболочка, `CurrentContent` для правой панели |
| `ChatListViewModel` | Singleton | Список чатов, подписка на SignalR-события |
| `ChatViewModel` | Transient | Один чат: отправка/получение/расшифровка сообщений |
| `ContactListViewModel` | Singleton | Список контактов |
| `AddContactViewModel` | Transient | Поиск и добавление контакта |
| `LoginViewModel` | Transient | Форма входа |
| `RegisterViewModel` | Transient | Форма регистрации |
| `SettingsViewModel` | Transient | Настройки (UTC-offset) |
| `KeyTransferViewModel` | Transient | Перенос E2EE-ключей на новое устройство через QR |
| `QrGeneratorViewModel` | Transient | Генерация QR-кода с публичным ключом |

### Сервисы

| Интерфейс | Реализация | Lifetime | Описание |
|-----------|-----------|---------|----------|
| `ISessionService` | `SessionService` | Singleton | Текущая сессия: userId, username, JWT |
| `ITokenStorage` | `TokenStorage` | Singleton | Сохранение/чтение `session.json` |
| `IRealtimeService` | `SignalRService` | Singleton | WebSocket-соединение с `/chatHub` |
| `INavigationService` | `NavigationService` | Singleton | Навигация между экранами |
| `ICryptoService` | `CryptoService` | Singleton | X25519 + AES-256-GCM |
| `IKeyStorageService` | `KeyStorageService` | Singleton | Хранение зашифрованной ключевой пары на диске |
| `ISharedSecretCache` | `SharedSecretCache` | Singleton | Кеш вычисленных ECDH-секретов на время сессии |
| `IAppSettingsService` | `AppSettingsService` | Singleton | Настройки (`appsettings.json`): UTC-offset |
| `IAuthApiService` | `AuthApiService` | Transient | REST: login, register |
| `IMessageApiService` | `MessageApiService` | Transient | REST: получение/отправка сообщений |
| `IContactApiService` | `ContactApiService` | Transient | REST: контакты |
| `IUserApiService` | `UserApiService` | Transient | REST: поиск пользователей, публичные ключи |
| `IQrScannerService` | `AndroidQrScannerService` / `DesktopQrScannerService` | Singleton | Сканирование QR-кода |

---

## E2EE Криптография (`FiweClient.Crypto`)

### Схема

```
Генерация ключей (один раз на устройство):
  X25519KeyPairGenerator → (PublicKey, PrivateKey)
  PublicKey  → публикуется на сервере
  PrivateKey → шифруется AES-256-GCM (ключ = PBKDF2-SHA256 из пароля, 100к итераций)
               и сохраняется в файл {userId}.dat

Общий секрет (на каждый контакт, кешируется):
  SharedSecret = ECDH(MyPrivate, ContactPublic)  → SHA-256 → 32 байта
  Свойство ECDH: оба участника получают одинаковый секрет без его передачи по сети

Шифрование сообщения:
  Nonce (12 байт, случайный) + AES-256-GCM(plaintext, SharedSecret)
  Формат: [12 байт nonce][ciphertext + 16 байт GCM-тег]  → base64

Расшифровка:
  base64 → разбить на nonce + ciphertext → AES-256-GCM decrypt
  При подделке/неверном ключе — InvalidCipherTextException
```

Сервер **никогда не видит** plaintext — только зашифрованный base64.

---

## SignalR (реалтайм)

```
Клиент подключается: /chatHub?access_token=<JWT>
При открытии чата: InvokeAsync("JoinChat", chatId)

Сервер вызывает: ReceiveMessage(chatId, recipients[], messageId)
ChatViewModel:
  1. Проверяет, что session.UserId есть в recipients
  2. Получает зашифрованное тело по messageId через REST
  3. Расшифровывает через ICryptoService
  4. Добавляет в список сообщений через Dispatcher.UIThread
```

При обрыве соединения — `WithAutomaticReconnect()` переподключает автоматически.

---

## REST API

Base URL: `https://fiwe-api-dgabh9axgnhagqhg.italynorth-01.azurewebsites.net/`

Все запросы через `BaseApiService`, который добавляет `Authorization: Bearer <token>`.

### Основные DTO

```csharp
// Авторизация
record LoginRequest(string UserName, string Password);
record LoginResponse(string Token);

// Чаты
record ChatDto(string ChatId, string Name, string? PictureUrl,
               DateTime LastActiveDateTime, string? LastMessagePreview, string? RecipientId);

// Сообщения (тело — зашифрованный base64)
record MessageDto(DateTime SentAt, string SenderObjectId, string MessageBody);
record SendMessageRequest(string ChatId, string MessageBody);

// Публичные ключи
record SetPublicKeyRequest(string PublicKeyBase64);
record PublicKeyResponse(string PublicKeyBase64);
```

---

## Аутентификация и сессия

1. После login сервер возвращает JWT
2. `LoginViewModel` извлекает `userId` из claim `nameid`/`nameidentifier`
3. Сессия сохраняется в `session.json` через `ITokenStorage`
4. При следующем запуске `TryAutoLoginAsync` проверяет `exp > now + 1мин`
5. Если токен протух — `session.json` очищается, пользователь идёт на LoginView

---

## Хранилище файлов

| Платформа | Путь |
|-----------|------|
| Desktop | `%AppData%/Fiwe/` |
| Android | `Context.FilesDir` |

Файлы в директории:
- `session.json` — JWT + userId + username
- `appsettings.json` — настройки (UTC-offset)
- `{userId}.dat` — зашифрованная ключевая пара X25519

---

## Сборка и запуск

```bash
# Desktop
dotnet build FiweClient/FiweClient.sln
dotnet run --project FiweClient/FiweClient.App.csproj

# Android APK
dotnet build FiweClient.Droid/FiweClient.Droid.csproj -f net8.0-android
```

Тестов нет. Проверка — ручная через запуск приложения.
