namespace Nameless.Serialization {

    public interface ISerializer {

        #region Methods

        byte[] Serialize(object? graph, SerializationOptions? options = null);
        
        object? Deserialize(Type? type, byte[]? buffer, SerializationOptions? options = null);
        
        void Serialize(Stream? stream, object? graph, SerializationOptions? options = null);

        object? Deserialize(Type? type, Stream? stream, SerializationOptions? options = null);

        #endregion
    }
}