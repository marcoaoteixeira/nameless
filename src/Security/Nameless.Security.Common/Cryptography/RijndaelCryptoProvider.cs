using System.Security.Cryptography;
using System.Text;

namespace Nameless.Security.Cryptography {

    /// <summary>
    /// This class uses a symmetric key algorithm (Rijndael/AES) to encrypt and
    /// decrypt data. As long as it is initialized with the same constructor
    /// parameters, the class will use the same key. Before performing encryption,
    /// the class can prepend random bytes to plain text and generate different
    /// encrypted values from the same plain text, encryption key, initialization
    /// vector, and other parameters. This class is thread-safe.
    /// </summary>
    /// <remarks>
    /// Be careful when performing encryption and decryption. There is a bug
    /// ("feature"?) in .NET Framework, which causes corruption of encryptor/
    /// decryptor if a cryptographic exception occurs during encryption/
    /// decryption operation. To correct the problem, re-initialize the class
    /// instance when a cryptographic exception occurs.
    /// </remarks>
    public sealed class RijndaelCryptoProvider : ICryptoProvider, IDisposable {

        #region Private Constants

        private const string DEFAULT_PASS_PHRASE = "29850952b3ef9f90";
        private const string DEFAULT_INITIALIZATION_VECTOR = "9e209040c863f84a";
        private const string DEFAULT_SALT = @"c11083b4b0a7743af748c85d343dfee9fbb8b2576c05f3a7f0d632b0926aadfc
2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824
08eac03b80adc33dc7d8fbe44b7c7b05d3a2c511166bdb43fcb710b03ba919e7
9e209040c863f84a31e719795b2577523954739fe5ed3b58a75cff2127075ed1
e4ba5cbd251c98e6cd1c23f126a3b81d8d8328abc95387229850952b3ef9f904
d1d3ec2e6f20fd420d50e2642992841d8338a314b8ea157c9e18477aaef226ab
5206b8b8a996cf5320cb12ca91c7b790fba9f030408efe83ebb83548dc3007bd
a49670c3c18b9e079b9cfaf51634f563dc8ae3070db2c4a8544305df1b60f007";

        // If key size is not specified, use the longest 256-bit key.
        private const KeySize DEFAULT_KEY_SIZE = KeySize.Large;

        // Do not allow salt to be longer than 255 bytes, because we have only
        // 1 byte to store its length.
        private const int MAXIMUN_ALLOWED_SALT_LENGTH = 255;

        // Do not allow salt to be smaller than 4 bytes, because we use the first
        // 4 bytes of salt to store its length.
        private const int MINIMUN_ALLOWED_SALT_LENGHT = 4;

        // Random salt value will be between 4 and 8 bytes long.
        private const int DEFAULT_MINIMUN_SALT_LENGHT = MINIMUN_ALLOWED_SALT_LENGHT;

        private const int DEFAULT_MAXIMUN_SALT_LENGHT = 8;

        #endregion

        #region Private Fields

        // These members will be used to perform encryption and decryption.
        private ICryptoTransform? _encryptor;
        private ICryptoTransform? _decryptor;
        private bool _disposed;

        #endregion

        #region Private Read-Only Fields

        // Use these members to save min and max salt lengths.
        private readonly int _minimunSaltLength = -1;
        private readonly int _maximunSaltLength = -1;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Use this constructor if you are planning to perform encryption/
        /// decryption with the key derived from the explicitly specified
        /// parameters.
        /// </summary>
        /// <param name="passPhrase">
        /// Passphrase from which a pseudo-random password will be derived.
        /// The derived password will be used to generate the encryption key
        /// Passphrase can be any string. In this example we assume that the
        /// passphrase is an ASCII string. Passphrase value must be kept in
        /// secret. Default value is: <value>DefaultPassPhrase</value>
        /// </param>
        /// <param name="initializationVector">
        /// Initialization vector (IV). This value is required to encrypt the
        /// first block of plaintext data. For RijndaelManaged class IV must be
        /// exactly 16 ASCII characters long. IV value does not have to be kept
        /// in secret. Default value is: <value>DefaultInitializationVector</value>
        /// </param>
        /// <param name="minimunSaltLength">
        /// Min size (in bytes) of randomly generated salt which will be added at
        /// the beginning of plain text before encryption is performed. When this
        /// value is less than 4, the default min value will be used (currently 4
        /// bytes).
        /// </param>
        /// <param name="maximunSaltLength">
        /// Max size (in bytes) of randomly generated salt which will be added at
        /// the beginning of plain text before encryption is performed. When this
        /// value is negative or greater than 255, the default max value will be
        /// used (currently 8 bytes). If max value is 0 (zero) or if it is smaller
        /// than the specified min value (which can be adjusted to default value),
        /// salt will not be used and plain text value will be encrypted as is.
        /// In this case, salt will not be processed during decryption either.
        /// </param>
        /// <param name="keySize">
        /// Size of symmetric key (in bits): 128, 192, or 256.
        /// </param>
        /// <param name="saltValue">
        /// Salt value used for password hashing during key generation. This is
        /// not the same as the salt we will use during encryption. This parameter
        /// can be any string.
        /// </param>
        /// <param name="passwordIterations">
        /// Number of iterations used to hash password. More iterations are
        /// considered more secure but may take longer.
        /// </param>
        public RijndaelCryptoProvider(string? passPhrase = null, string? initializationVector = null, int minimunSaltLength = DEFAULT_MINIMUN_SALT_LENGHT, int maximunSaltLength = DEFAULT_MAXIMUN_SALT_LENGHT, KeySize keySize = DEFAULT_KEY_SIZE, string? saltValue = null, int passwordIterations = 1) {
            // Save min salt length; set it to default if invalid value is passed.
            _minimunSaltLength = minimunSaltLength < MINIMUN_ALLOWED_SALT_LENGHT
                ? DEFAULT_MINIMUN_SALT_LENGHT
                : minimunSaltLength;

            // Save max salt length; set it to default if invalid value is passed.
            _maximunSaltLength = maximunSaltLength < MAXIMUN_ALLOWED_SALT_LENGTH
                ? DEFAULT_MAXIMUN_SALT_LENGHT
                : maximunSaltLength;

            // Initialization vector converted to a byte array.
            // Get bytes of initialization vector.
            var initializationVectorBytes = Encoding.ASCII.GetBytes(initializationVector ?? DEFAULT_INITIALIZATION_VECTOR);

            // Salt used for password hashing (to generate the key, not during
            // encryption) converted to a byte array.
            // Get bytes of salt (used in hashing).
            var saltValueBytes = Encoding.ASCII.GetBytes(saltValue ?? DEFAULT_SALT);

            byte[]? keyBytes = null;
            // Generate password, which will be used to derive the key.
            using (var password = new Rfc2898DeriveBytes(passPhrase ?? DEFAULT_PASS_PHRASE, saltValueBytes, passwordIterations)) {
                // Convert key to a byte array adjusting the size from bits to bytes.
                keyBytes = password.GetBytes((int)keySize / 8);
            }

            // Initialize Rijndael key object.
            using var symmetricKey = Aes.Create();
            // If we do not have initialization vector, we cannot use the CBC mode.
            // The only alternative is the ECB mode (which is not as good).
            symmetricKey.Mode = initializationVectorBytes.Length == 0 ? CipherMode.ECB : CipherMode.CBC;

            _encryptor = symmetricKey.CreateEncryptor(keyBytes, initializationVectorBytes);
            _decryptor = symmetricKey.CreateDecryptor(keyBytes, initializationVectorBytes);
        }

        #endregion

        #region Destructors

        /// <summary>
        /// Destructor.
        /// </summary>
        ~RijndaelCryptoProvider() {
            Dispose(false);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds an array of randomly generated bytes at the beginning of the
        /// array holding original plain text value.
        /// </summary>
        /// <param name="plainTextBytes">
        /// Byte array containing original plain text value.
        /// </param>
        /// <returns>
        /// Either original array of plain text bytes (if salt is not used) or a
        /// modified array containing a randomly generated salt added at the
        /// beginning of the plain text bytes.
        /// </returns>
        private byte[] AddSalt(byte[] plainTextBytes) {
            // The max salt value of 0 (zero) indicates that we should not use
            // salt. Also do not use salt if the max salt value is smaller than
            // the min value.
            if (_maximunSaltLength == 0 || _maximunSaltLength < _minimunSaltLength) {
                return plainTextBytes;
            }

            // Generate the salt.
            var saltBytes = GenerateSalt();

            // Allocate array which will hold salt and plain text bytes.
            var plainTextBytesWithSalt = new byte[plainTextBytes.Length + saltBytes.Length];

            // First, copy salt bytes.
            Array.Copy(saltBytes, plainTextBytesWithSalt, saltBytes.Length);

            // Append plain text bytes to the salt value.
            Array.Copy(plainTextBytes, 0, plainTextBytesWithSalt, saltBytes.Length, plainTextBytes.Length);

            return plainTextBytesWithSalt;
        }

        /// <summary>
        /// Generates an array holding cryptographically strong bytes.
        /// </summary>
        /// <returns>
        /// Array of randomly generated bytes.
        /// </returns>
        /// <remarks>
        /// Salt size will be defined at random or exactly as specified by the
        /// minSlatLen and maxSaltLen parameters passed to the object constructor.
        /// The first four bytes of the salt array will contain the salt length
        /// split into four two-bit pieces.
        /// </remarks>
        private byte[] GenerateSalt() {
            // We don't have the length, yet.
            // If min and max salt values are the same, it should not be random.
            var saltLength = _minimunSaltLength != _maximunSaltLength
                ? GenerateRandomNumber(_minimunSaltLength, _maximunSaltLength)
                : _minimunSaltLength;

            // Allocate byte array to hold our salt.
            var salt = new byte[saltLength];

            // Populate salt with cryptographically strong bytes.
            using (var rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(salt);
            }

            // Split salt length (always one byte) into four two-bit pieces and
            // store these pieces in the first four bytes of the salt array.
            salt[0] = (byte)((salt[0] & 0xfc) | (saltLength & 0x03));
            salt[1] = (byte)((salt[1] & 0xf3) | (saltLength & 0x0c));
            salt[2] = (byte)((salt[2] & 0xcf) | (saltLength & 0x30));
            salt[3] = (byte)((salt[3] & 0x3f) | (saltLength & 0xc0));

            return salt;
        }

        private void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }

            if (disposing) {
                if (_encryptor != null) {
                    _encryptor.Dispose();
                }

                if (_decryptor != null) {
                    _decryptor.Dispose();
                }
            }

            _encryptor = null;
            _decryptor = null;
            _disposed = true;
        }

        private void BlockAccesAfterDispose() {
            if (_disposed) {
                throw new ObjectDisposedException(nameof(RijndaelCryptoProvider));
            }
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Generates random integer.
        /// </summary>
        /// <param name="minimunValue">
        /// Min value (inclusive).
        /// </param>
        /// <param name="maximunValue">
        /// Max value (inclusive).
        /// </param>
        /// <returns>
        /// Random integer value between the min and max values (inclusive).
        /// </returns>
        /// <remarks>
        /// This methods overcomes the limitations of .NET Framework's Random
        /// class, which - when initialized multiple times within a very short
        /// period of time - can generate the same "random" number.
        /// </remarks>
        private static int GenerateRandomNumber(int minimunValue, int maximunValue) {
            // We will make up an integer seed from 4 bytes of this array.
            var randomBytes = new byte[4];

            // Generate 4 random bytes.
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            // Convert four random bytes into a positive integer value.
            var seed = ((randomBytes[0] & 0x7f) << 24) |
                (randomBytes[1] << 16) |
                (randomBytes[2] << 8) |
                (randomBytes[3]);

            // Now, this looks more like real randomization.
            // And, calculate a random number.
            return new Random(seed).Next(minimunValue, maximunValue + 1);
        }

        #endregion

        #region ICryptoProvider Members

        /// <inheritdoc />
        public byte[]? Encrypt(Stream? stream, CryptoOptions? options = null) {
            BlockAccesAfterDispose();

            if (stream == null) { return Array.Empty<byte>(); }

            // Let's make cryptographic operations thread-safe.
            lock (this) {
                try {
                    // To perform encryption, we must use the Write mode.
                    using var memoryStream = new MemoryStream();
                    using var cryptoStream = new CryptoStream(memoryStream, _encryptor!, CryptoStreamMode.Write);
                    var value = stream.ToByteArray();
                    var valueWithSalt = AddSalt(value);

                    // Start encrypting data.
                    cryptoStream.Write(valueWithSalt, 0, valueWithSalt.Length);
                    // Finish the encryption operation.
                    cryptoStream.FlushFinalBlock();
                    // Return encrypted data.
                    return memoryStream.ToArray();
                } catch (Exception) { Dispose(); throw; }
            }
        }

        /// <inheritdoc />
        public byte[]? Decrypt(Stream? stream, CryptoOptions? options = null) {
            BlockAccesAfterDispose();

            if (stream == null) { return Array.Empty<byte>(); }

            byte[] decryptedBytes;
            var decryptedByteCount = 0;
            var saltLength = 0;

            // Let's make cryptographic operations thread-safe.
            lock (this) {
                try {
                    var value = stream.ToByteArray();

                    // Since we do not know how big decrypted value will be, use the same
                    // size as cipher text. Cipher text is always longer than plain text
                    // (in block cipher encryption), so we will just use the number of
                    // decrypted data byte after we know how big it is.
                    decryptedBytes = new byte[value.Length];

                    // To perform decryption, we must use the Read mode.
                    using var memoryStream = new MemoryStream(value);
                    using var cryptoStream = new CryptoStream(memoryStream, _decryptor!, CryptoStreamMode.Read);
                    // Decrypting data and get the count of plain text bytes.
                    decryptedByteCount = cryptoStream.Read(decryptedBytes, 0, decryptedBytes.Length);
                } catch (Exception) { Dispose(); throw; }
            }

            // If we are using salt, get its length from the first 4 bytes of plain
            // text data.
            if (_maximunSaltLength > 0 && _maximunSaltLength >= _minimunSaltLength) {
                saltLength = (decryptedBytes[0] & 0x03) |
                             (decryptedBytes[1] & 0x0c) |
                             (decryptedBytes[2] & 0x30) |
                             (decryptedBytes[3] & 0xc0);
            }

            // Allocate the byte array to hold the original plain text (without salt).
            var plainTextBytes = new byte[decryptedByteCount - saltLength];

            // Copy original plain text discarding the salt value if needed.
            Array.Copy(decryptedBytes, saltLength, plainTextBytes, 0, decryptedByteCount - saltLength);

            // Return original plain text value.
            return plainTextBytes;
        }

        #endregion

        #region IDisposable Members

        /// <inheritdoc />
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
