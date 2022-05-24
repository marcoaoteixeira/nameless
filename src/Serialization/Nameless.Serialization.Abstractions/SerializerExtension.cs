namespace Nameless.Serialization {

    public static class SerializerExtension {

        #region Public Static Methods

        public static T Deserialize<T>(this ISerializer self, byte[]? buffer, SerializationOptions? options = null) {
            if (self == null) { return default!; }

            Ensure.NotNull(buffer, nameof(buffer));

            var result = self.Deserialize(typeof(T), buffer, options);
            if (result == null) { return default!; }

            return (T)result;
        }

        public static T Deserialize<T>(this ISerializer self, Stream? stream, SerializationOptions? options = null) {
            if (self == null) { return default!; }

            Ensure.NotNull(stream, nameof(stream));

            var result = self.Deserialize(typeof(T), stream, options);
            if (result == null) { return default!; }

            return (T)result;
        }

        #endregion
    }
}