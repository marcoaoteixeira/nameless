using System.Text;

namespace Nameless {

    /// <summary>
    /// <see cref="Stream"/> extension methods.
    /// </summary>
    public static class StreamExtension {

        #region Public Static Methods

        /// <summary>
        /// Tries to read a stream to a string value.
        /// </summary>
        /// <param name="self">The stream</param>
        /// <param name="encoding">The encoding. Default is <see cref="Encoding.ASCII" /></param>
        /// <returns>The stream as a string.</returns>
        public static string ToText(this Stream self, Encoding? encoding = null) => (encoding ?? Encoding.ASCII).GetString(self.ToByteArray());

        /// <summary>
        /// Converts a <see cref="Stream"/> into a byte array.
        /// </summary>
        /// <param name="self">The source <see cref="Stream"/></param>
        /// <param name="bufferSize">The buffer size. Default is <see cref="BufferSize.Small"/></param>
        /// <returns>A byte array representing the <see cref="Stream"/>.</returns>
        public static byte[] ToByteArray(this Stream self, BufferSize bufferSize = BufferSize.Small) {
            if (self == null) { return Array.Empty<byte>(); }
            if (!self.CanRead) { return Array.Empty<byte>(); }
            if (!Enum.IsDefined(bufferSize)) { bufferSize = BufferSize.Small; }

            // Return faster...
            if (self is MemoryStream ms) { return ms.ToArray(); }

            using var memoryStream = new MemoryStream();
            var allocationSize = (int)bufferSize;
            var buffer = new byte[allocationSize];
            int count;
            while ((count = self.Read(buffer, offset: 0, count: allocationSize)) > 0) {
                memoryStream.Write(buffer, offset: 0, count: count);
            }
            return memoryStream.ToArray();
        }

        #endregion
    }
}