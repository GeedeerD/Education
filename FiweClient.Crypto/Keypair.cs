namespace FiweClient.Crypto
{
    /// <summary>
    /// X25519 ключевая пара пользователя.
    /// PublicKey  — отправляется на сервер, виден всем.
    /// PrivateKey — НИКОГДА не покидает устройство.
    /// </summary>
    public class KeyPair
    {
        public byte[] PublicKey { get; init; }
        public byte[] PrivateKey { get; init; }

        public string PublicKeyBase64 => Convert.ToBase64String(PublicKey);

        public KeyPair(byte[] publicKey, byte[] privateKey)
        {
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }
    }
}