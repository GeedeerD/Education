using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Text;

namespace FiweClient.Crypto
{
    public interface ICryptoService
    {
        /// <summary>Генерирует новую X25519 ключевую пару</summary>
        KeyPair GenerateKeyPair();

        /// <summary>
        /// Вычисляет общий секрет из своего приватного и публичного ключа собеседника.
        /// Результат кешируется в ChatViewModel на время сессии.
        /// </summary>
        byte[] ComputeSharedSecret(byte[] myPrivateKey, byte[] contactPublicKey);
        byte[] ComputeSharedSecret(string myPrivateKeyBase64, string contactPublicKeyBase64);

        /// <summary>Шифрует текст, возвращает base64-строку для отправки на сервер</summary>
        string Encrypt(string plainText, byte[] sharedSecret);
        string Encrypt(string plainText, string sharedSecretBase64);

        /// <summary>Расшифровывает base64-строку с сервера, возвращает текст</summary>
        string Decrypt(string encryptedBase64, byte[] sharedSecret);
        string Decrypt(string encryptedBase64, string sharedSecretBase64);
    }
    /// <summary>
    /// Весь криптографический слой приложения.
    ///
    /// Схема работы:
    /// 1. Каждый пользователь генерирует X25519 ключевую пару.
    /// 2. PublicKey публикуется на сервере.
    /// 3. SharedSecret = ECDH(MyPrivateKey, ContactPublicKey)
    ///    — одинаков у обоих участников, сервер его не знает.
    /// 4. Сообщения шифруются AES-256-GCM на SharedSecret.
    /// </summary>
    public class CryptoService : ICryptoService
    {
        private const int AesKeySize = 32;    // 256 бит
        private const int GcmNonceSize = 12;  // 96 бит — стандарт для GCM
        private const int GcmTagSize = 128;   // 128 бит тег аутентификации

        private readonly SecureRandom _random = new();

        // ─────────────────────────────────────────────
        // 1. Генерация ключевой пары
        // ─────────────────────────────────────────────

        /// <summary>
        /// Генерирует новую X25519 ключевую пару.
        /// Вызывается один раз при первом запуске приложения.
        /// </summary>
        public KeyPair GenerateKeyPair()
        {
            var generator = new X25519KeyPairGenerator();
            generator.Init(new X25519KeyGenerationParameters(_random));

            var keyPair = generator.GenerateKeyPair();

            var privateKey = ((X25519PrivateKeyParameters)keyPair.Private).GetEncoded();
            var publicKey = ((X25519PublicKeyParameters)keyPair.Public).GetEncoded();

            return new KeyPair(publicKey, privateKey);
        }

        // ─────────────────────────────────────────────
        // 2. Вычисление общего секрета
        // ─────────────────────────────────────────────

        /// <summary>
        /// Вычисляет SharedSecret из своего приватного ключа и публичного ключа собеседника.
        /// 
        /// Магия ECDH:
        ///   ECDH(Alice.Private, Bob.Public) == ECDH(Bob.Private, Alice.Public)
        /// Сервер знает оба публичных ключа, но вычислить SharedSecret без приватного — невозможно.
        /// </summary>
        public byte[] ComputeSharedSecret(byte[] myPrivateKey, byte[] contactPublicKey)
        {
            var privateKeyParams = new X25519PrivateKeyParameters(myPrivateKey);
            var publicKeyParams = new X25519PublicKeyParameters(contactPublicKey);

            var agreement = new X25519Agreement();
            agreement.Init(privateKeyParams);

            var sharedSecret = new byte[agreement.AgreementSize]; // 32 байта
            agreement.CalculateAgreement(publicKeyParams, sharedSecret, 0);

            // Прогоняем через SHA-256 для равномерного распределения битов
            return HashSharedSecret(sharedSecret);
        }

        /// <summary>
        /// Перегрузка: принимает base64-строки (удобно при работе с API)
        /// </summary>
        public byte[] ComputeSharedSecret(string myPrivateKeyBase64, string contactPublicKeyBase64)
        {
            return ComputeSharedSecret(
                Convert.FromBase64String(myPrivateKeyBase64),
                Convert.FromBase64String(contactPublicKeyBase64)
            );
        }

        // ─────────────────────────────────────────────
        // 3. Шифрование / Расшифровка
        // ─────────────────────────────────────────────

        /// <summary>
        /// Шифрует текст сообщения.
        /// 
        /// Формат результата (base64):
        ///   [ 12 байт Nonce ][ зашифрованные данные + 16 байт GCM тег ]
        /// 
        /// Nonce генерируется случайно для каждого сообщения —
        /// одинаковые тексты дают разный зашифрованный результат.
        /// </summary>
        public string Encrypt(string plainText, byte[] sharedSecret)
        {
            var nonce = GenerateNonce();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);

            var cipher = new GcmBlockCipher(new Org.BouncyCastle.Crypto.Engines.AesEngine());
            var parameters = new AeadParameters(new KeyParameter(sharedSecret), GcmTagSize, nonce);
            cipher.Init(true, parameters); // true = encrypt

            var cipherBytes = new byte[cipher.GetOutputSize(plainBytes.Length)];
            var len = cipher.ProcessBytes(plainBytes, 0, plainBytes.Length, cipherBytes, 0);
            cipher.DoFinal(cipherBytes, len);

            // Склеиваем nonce + зашифрованные данные в один массив
            var result = new byte[GcmNonceSize + cipherBytes.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, GcmNonceSize);
            Buffer.BlockCopy(cipherBytes, 0, result, GcmNonceSize, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// Расшифровывает сообщение.
        /// Если сообщение подделано или ключ неверный — выбросит InvalidCipherTextException.
        /// </summary>
        public string Decrypt(string encryptedBase64, byte[] sharedSecret)
        {
            var allBytes = Convert.FromBase64String(encryptedBase64);

            // Разбиваем на nonce и зашифрованные данные
            var nonce = new byte[GcmNonceSize];
            var cipherBytes = new byte[allBytes.Length - GcmNonceSize];

            Buffer.BlockCopy(allBytes, 0, nonce, 0, GcmNonceSize);
            Buffer.BlockCopy(allBytes, GcmNonceSize, cipherBytes, 0, cipherBytes.Length);

            var cipher = new GcmBlockCipher(new Org.BouncyCastle.Crypto.Engines.AesEngine());
            var parameters = new AeadParameters(new KeyParameter(sharedSecret), GcmTagSize, nonce);
            cipher.Init(false, parameters); // false = decrypt

            var plainBytes = new byte[cipher.GetOutputSize(cipherBytes.Length)];
            var len = cipher.ProcessBytes(cipherBytes, 0, cipherBytes.Length, plainBytes, 0);
            cipher.DoFinal(plainBytes, len);

            return Encoding.UTF8.GetString(plainBytes);
        }

        /// <summary>
        /// Перегрузки с base64-ключом (удобно хранить ключи как строки)
        /// </summary>
        public string Encrypt(string plainText, string sharedSecretBase64) =>
            Encrypt(plainText, Convert.FromBase64String(sharedSecretBase64));

        public string Decrypt(string encryptedBase64, string sharedSecretBase64) =>
            Decrypt(encryptedBase64, Convert.FromBase64String(sharedSecretBase64));

        // ─────────────────────────────────────────────
        // Вспомогательные методы
        // ─────────────────────────────────────────────

        private byte[] GenerateNonce()
        {
            var nonce = new byte[GcmNonceSize];
            _random.NextBytes(nonce);
            return nonce;
        }

        /// <summary>
        /// Хешируем raw SharedSecret через SHA-256.
        /// Это стандартная практика — raw ECDH output не всегда равномерно распределён.
        /// </summary>
        private static byte[] HashSharedSecret(byte[] rawSecret)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            return sha256.ComputeHash(rawSecret);
        }
    }
}