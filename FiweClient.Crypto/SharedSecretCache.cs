using System.Collections.Concurrent;

namespace FiweClient.Crypto
{
    public interface ISharedSecretCache
    {
        bool TryGet(string contactUserId, out byte[] sharedSecret);
        void Set(string contactUserId, byte[] sharedSecret);
        void Clear();
    }

    /// <summary>
    /// Кеш SharedSecret-ов на время сессии.
    ///
    /// SharedSecret вычисляется один раз при открытии чата
    /// и хранится в памяти до закрытия приложения.
    /// На диск никогда не записывается.
    ///
    /// Ключ кеша: userId собеседника
    /// </summary>
    public class SharedSecretCache : ISharedSecretCache
    {
        private readonly ConcurrentDictionary<string, byte[]> _cache = new();

        public bool TryGet(string contactUserId, out byte[] sharedSecret)
            => _cache.TryGetValue(contactUserId, out sharedSecret!);

        public void Set(string contactUserId, byte[] sharedSecret)
            => _cache[contactUserId] = sharedSecret;

        /// <summary>
        /// Очищает кеш при выходе из аккаунта.
        /// Секреты удаляются из памяти.
        /// </summary>
        public void Clear() => _cache.Clear();
    }
}