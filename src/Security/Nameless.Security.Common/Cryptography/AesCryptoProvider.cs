using System.Security.Cryptography;

namespace Nameless.Security.Cryptography {

    [Singleton]
	public sealed class AesCryptoProvider : ICryptoProvider {

		#region Public Static Properties

		public static ICryptoProvider Instance { get; } = new AesCryptoProvider();

		#endregion

		#region Static Constructors

		// Explicit static constructor to tell the C# compiler
		// not to mark type as beforefieldinit
		static AesCryptoProvider() { }

		#endregion

		#region Private Constructors

		// Prevents the class from being constructed.
		private AesCryptoProvider() { }

		#endregion

		#region ICryptoProvider Members

		public byte[]? Encrypt(Stream? stream, CryptoOptions? options = null) {
			if (stream == null) { return Array.Empty<byte>(); }

			var opts = options ?? new();
			var text = stream.ToText(opts.Encoding);
			var iv = new byte[16];

			using var aes = Aes.Create();
			aes.Key = opts.Encoding.GetBytes(opts.Key);
			aes.IV = iv;

			using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
			using var memoryStream = new MemoryStream();
			using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
			using var streamWriter = new StreamWriter(cryptoStream);
			streamWriter.Write(text);

			return memoryStream.ToArray();
		}

		public byte[]? Decrypt(Stream? stream, CryptoOptions? options = null) {
			if (stream == null) { return Array.Empty<byte>(); }

			var opts = options ?? new();
			var text = stream.ToText(opts.Encoding);
			var iv = new byte[16];

			var buffer = opts.Encoding.GetBytes(text);
			using var aes = Aes.Create();
			aes.Key = opts.Encoding.GetBytes(opts.Key);
			aes.IV = iv;

			using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
			using var memoryStream = new MemoryStream(buffer);
			using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
			using var streamReader = new StreamReader(cryptoStream);
			var decrypted = streamReader.ReadToEnd();

			return opts.Encoding.GetBytes(decrypted);
		}

		#endregion
	}
}
