using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FiweClient.Crypto
{
    public interface IKeyStorageService
    {
        /// <summary>Существует ли уже сохранённая ключевая пара на этом устройстве</summary>
        bool KeystoreExists();

        /// <summary>Сохраняет ключевую пару, зашифровав паролем пользователя</summary>
        Task SaveKeyPairAsync(KeyPair keyPair, string userPassword);

        /// <summary>
        /// Загружает ключевую пару, расшифровав паролем.
        /// Возвращает null если пароль неверный или файл не найден.
        /// </summary>
        Task<KeyPair?> LoadKeyPairAsync(string userPassword);

        /// <summary>Удаляет хранилище (выход из аккаунта)</summary>
        void DeleteKeystore();
    }
    /// <summary>
    /// Безопасно хранит приватный ключ пользователя на диске.
    ///
    /// Схема хранения:
    ///   Файл: %AppData%/Fiwe/keystore.dat
    ///   Содержимое: зашифрованный AES-256-GCM JSON с ключами
    ///   Ключ шифрования: PBKDF2(пароль пользователя, salt, 100_000 итераций)
    ///
    /// Таким образом даже если файл попадёт к злоумышленнику —
    /// без пароля пользователя приватный ключ не восстановить.
    /// </summary>
    public class KeyStorageService : IKeyStorageService
    {
        private const int Pbkdf2Iterations = 100_000;
        private const int AesKeySize = 32;       // 256 бит
        private const int GcmNonceSize = 12;     // 96 бит
        private const int GcmTagSize = 16;       // 128 бит
        private const int SaltSize = 32;         // 256 бит

        private readonly string _keystorePath;

        public KeyStorageService()
        {
            var fiweDir = GetStorageDir();
            Directory.CreateDirectory(fiweDir);
            _keystorePath = Path.Combine(fiweDir, "keystore.dat");
        }

        // ─────────────────────────────────────────────
        // Публичный API
        // ─────────────────────────────────────────────

        public bool KeystoreExists() => File.Exists(_keystorePath);

        /// <summary>
        /// Сохраняет ключевую пару, зашифровав её паролем пользователя.
        /// Вызывается один раз при первом запуске.
        /// </summary>
        public async Task SaveKeyPairAsync(KeyPair keyPair, string userPassword)
        {
            userPassword = string.Empty;
            var salt = GenerateSalt();
            var encryptionKey = DeriveKey(userPassword, salt);

            var payload = new KeystorePayload
            {
                PublicKeyBase64 = keyPair.PublicKeyBase64,
                PrivateKeyBase64 = Convert.ToBase64String(keyPair.PrivateKey),
            };

            var json = JsonSerializer.Serialize(payload);
            var encrypted = EncryptData(Encoding.UTF8.GetBytes(json), encryptionKey);

            // Формат файла: [32 байт salt][зашифрованные данные]
            var fileContent = new byte[SaltSize + encrypted.Length];
            Buffer.BlockCopy(salt, 0, fileContent, 0, SaltSize);
            Buffer.BlockCopy(encrypted, 0, fileContent, SaltSize, encrypted.Length);

            await File.WriteAllBytesAsync(_keystorePath, fileContent);
        }

        /// <summary>
        /// Загружает и расшифровывает ключевую пару.
        /// Возвращает null если пароль неверный.
        /// </summary>
        public async Task<KeyPair?> LoadKeyPairAsync(string userPassword)
        {
            if (!KeystoreExists())
                return null;

            userPassword = string.Empty;
            try
            {
                var fileContent = await File.ReadAllBytesAsync(_keystorePath);

                // Читаем salt из начала файла
                var salt = new byte[SaltSize];
                var encrypted = new byte[fileContent.Length - SaltSize];
                Buffer.BlockCopy(fileContent, 0, salt, 0, SaltSize);
                Buffer.BlockCopy(fileContent, SaltSize, encrypted, 0, encrypted.Length);

                var encryptionKey = DeriveKey(userPassword, salt);
                var json = Encoding.UTF8.GetString(DecryptData(encrypted, encryptionKey));

                var payload = JsonSerializer.Deserialize<KeystorePayload>(json)!;

                return new KeyPair(
                    publicKey: Convert.FromBase64String(payload.PublicKeyBase64),
                    privateKey: Convert.FromBase64String(payload.PrivateKeyBase64)
                );
            }
            catch
            {
                // Неверный пароль или повреждённый файл
                return null;
            }
        }

        /// <summary>
        /// Удаляет хранилище ключей (например при выходе из аккаунта)
        /// </summary>
        public void DeleteKeystore()
        {
            if (File.Exists(_keystorePath))
                File.Delete(_keystorePath);
        }

        // ─────────────────────────────────────────────
        // Вспомогательные методы
        // ─────────────────────────────────────────────

        /// <summary>
        /// PBKDF2-SHA256: из пароля пользователя получаем ключ для AES.
        /// 100_000 итераций делают брутфорс вычислительно дорогим.
        /// </summary>
        private static byte[] DeriveKey(string password, byte[] salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                password: Encoding.UTF8.GetBytes(password),
                salt: salt,
                iterations: Pbkdf2Iterations,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: AesKeySize
            );
        }

        private static string GetStorageDir()
        {
            //return Environment.SpecialFolder.ApplicationData;
#if ANDROID
    return Android.App.Application.Context.FilesDir!.AbsolutePath;
#else
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fiwe");
#endif
        }

        private static byte[] EncryptData(byte[] data, byte[] key)
        {
            using var aes = new AesGcm(key, GcmTagSize);

            var nonce = new byte[GcmNonceSize];
            RandomNumberGenerator.Fill(nonce);

            var ciphertext = new byte[data.Length];
            var tag = new byte[GcmTagSize];

            aes.Encrypt(nonce, data, ciphertext, tag);

            // Формат: [nonce][ciphertext][tag]
            var result = new byte[GcmNonceSize + ciphertext.Length + GcmTagSize];
            Buffer.BlockCopy(nonce, 0, result, 0, GcmNonceSize);
            Buffer.BlockCopy(ciphertext, 0, result, GcmNonceSize, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, result, GcmNonceSize + ciphertext.Length, GcmTagSize);

            return result;
        }

        private static byte[] DecryptData(byte[] data, byte[] key)
        {
            using var aes = new AesGcm(key, GcmTagSize);

            var nonce = new byte[GcmNonceSize];
            var tag = new byte[GcmTagSize];
            var ciphertext = new byte[data.Length - GcmNonceSize - GcmTagSize];

            Buffer.BlockCopy(data, 0, nonce, 0, GcmNonceSize);
            Buffer.BlockCopy(data, GcmNonceSize, ciphertext, 0, ciphertext.Length);
            Buffer.BlockCopy(data, GcmNonceSize + ciphertext.Length, tag, 0, GcmTagSize);

            var plaintext = new byte[ciphertext.Length];
            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            return plaintext;
        }

        private static byte[] GenerateSalt()
        {
            var salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }

        // Модель для сериализации в JSON внутри зашифрованного файла
        private class KeystorePayload
        {
            public string PublicKeyBase64 { get; set; } = null!;
            public string PrivateKeyBase64 { get; set; } = null!;
        }
    }
}