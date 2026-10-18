using System.Security.Cryptography;

namespace Nameless.Compression.Internals;

/// <summary>
///     Computes and verifies archive checksums: SHA-256, or HMAC-SHA256 when
///     a secret key is configured.
/// </summary>
internal sealed class ArchiveChecksum {
    /// <summary>
    ///     RFC 2104: the key should be at least as long as the hash output.
    /// </summary>
    public const int MinimumKeyLength = HMACSHA256.HashSizeInBytes;

    private const int HEX_LENGTH = SHA256.HashSizeInBytes * 2;

    private readonly byte[]? _key;

    /// <summary>
    ///     Gets the checksum algorithm.
    /// </summary>
    public ChecksumAlgorithm Algorithm => _key is null
        ? ChecksumAlgorithm.Sha256
        : ChecksumAlgorithm.HmacSha256;

    /// <exception cref="ArgumentException">
    ///     <paramref name="hmacKey"/> is shorter than
    ///     <see cref="MinimumKeyLength"/> bytes.
    /// </exception>
    public ArchiveChecksum(byte[]? hmacKey) {
        if (hmacKey is null) { return; }

        if (hmacKey.Length < MinimumKeyLength) {
            throw new ArgumentException(
                message: $"The HMAC key must be at least {MinimumKeyLength} bytes long.",
                paramName: nameof(hmacKey)
            );
        }

        // Copied so the caller can neither mutate nor accidentally clear
        // it afterward.
        _key = (byte[])hmacKey.Clone();
    }

    /// <param name="hmacKey">
    ///     Base64 of at least <see cref="MinimumKeyLength"/> random bytes, or
    ///     <see langword="null"/> for plain SHA-256.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     <paramref name="hmacKey"/> is empty or white space, is not valid
    ///     Base64, or decodes to fewer than <see cref="MinimumKeyLength"/>
    ///     bytes. The message never contains the key.
    /// </exception>
    public ArchiveChecksum(string? hmacKey) {
        if (hmacKey is null) { return; }

        _key = DecodeKey(hmacKey);
    }

    /// <summary>
    ///     Accepts <see langword="null"/> (no verification) or a 64-character
    ///     hex string.
    /// </summary>
    /// <param name="checksum">
    ///     The checksum value.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     <paramref name="checksum"/> is not <see langword="null"/> and not
    ///     <see cref="IsValid">valid</see>.
    /// </exception>
    public static void Validate(string? checksum) {
        if (!IsValid(checksum)) {
            throw new ArgumentException(
                message: InvalidChecksumMessage,
                paramName: nameof(checksum)
            );
        }
    }

    /// <summary>
    ///     Whether <see cref="Validate"/> accepts the value.
    /// </summary>
    /// <param name="checksum">
    ///     The checksum value.
    /// </param>
    public static bool IsValid(string? checksum) {
        return checksum is null ||
               (checksum.Length == HEX_LENGTH && checksum.All(char.IsAsciiHexDigit));
    }

    /// <summary>
    ///     Describes the expected checksum format.
    /// </summary>
    public static string InvalidChecksumMessage => $"The checksum must be {HEX_LENGTH} hexadecimal characters (SHA-256 or HMAC-SHA256).";

    /// <summary>
    ///     Converts the byte array to a hex-string value.
    /// </summary>
    /// <param name="hash">
    ///     The byte array.
    /// </param>
    /// <returns>
    ///     The hex-string representation.
    /// </returns>
    public static string ToHex(byte[] hash) {
        return Convert.ToHexStringLower(hash);
    }

    /// <summary>
    ///     Constant-time comparison, so response timing reveals nothing
    ///     about a keyed value.
    /// </summary>
    /// <param name="expected">
    ///     A value already accepted by <see cref="Validate"/>.
    /// </param>
    /// <param name="actual">
    ///     The actual value.
    /// </param>
    public static bool Matches(string expected, byte[] actual) {
        return CryptographicOperations.FixedTimeEquals(
            left: Convert.FromHexString(expected),
            right: actual
        );
    }

    public async Task<byte[]> ComputeAsync(Stream source, CancellationToken cancellationToken) {
        return _key is null
            ? await SHA256.HashDataAsync(source, cancellationToken).ConfigureAwait(false)
            : await HMACSHA256.HashDataAsync(_key, source, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Builds the mismatch exception. The computed value is withheld for
    ///     HMAC: revealing the correct HMAC of attacker-supplied data would
    ///     let the attacker forge it.
    /// </summary>
    public ChecksumMismatchException Mismatch(string expected, byte[] actual) {
        return new ChecksumMismatchException(expected, _key is null ? ToHex(actual) : null);
    }

    private static byte[] DecodeKey(string hmacKey) {
        if (string.IsNullOrWhiteSpace(hmacKey)) {
            throw InvalidKey("The HMAC key is empty. Remove the setting to disable HMAC, or provide a valid key.");
        }

        byte[] key;

        try {
            // Ignores surrounding white space and line breaks; rejects
            // anything else outside the standard Base64 alphabet.
            key = Convert.FromBase64String(hmacKey);
        }
        catch (FormatException) {
            // Not chained: the inner exception adds nothing and must never
            // risk echoing the value.
            throw InvalidKey("The HMAC key is not valid Base64.");
        }

        if (key.Length >= MinimumKeyLength) {
            return key;
        }

        var length = key.Length;

        CryptographicOperations.ZeroMemory(key);

        throw InvalidKey($"The HMAC key decodes to {length} bytes; at least {MinimumKeyLength} are required.");
    }

    #pragma warning disable CA2208
    private static ArgumentException InvalidKey(string reason) {
        return new ArgumentException(
            message: $"{reason} Expected Base64 of at least {MinimumKeyLength} random bytes (e.g. 'openssl rand -base64 {MinimumKeyLength}').",
            paramName: nameof(ZipCompressorOptions.HmacKey)
        );
    }
    #pragma warning restore CA2208
}
