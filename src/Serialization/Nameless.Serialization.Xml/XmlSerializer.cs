using MS_XmlSerializer = System.Xml.Serialization.XmlSerializer;
using MS_XmlSerializerNamespaces = System.Xml.Serialization.XmlSerializerNamespaces;

namespace Nameless.Serialization.Xml {

    public sealed class XmlSerializer : ISerializer {

        #region Private Static Methods

        private static void Serialize(Stream stream, object graph, IDictionary<string, string>? namespaces = null) {
            Ensure.NotNull(stream, nameof(stream));
            Ensure.NotNull(graph, nameof(graph));

            var xmlSerializer = new MS_XmlSerializer(graph.GetType());
            var xmlSerializerNamespaces = new MS_XmlSerializerNamespaces();
            if (namespaces != null && namespaces.Any()) {
                foreach (var kvp in namespaces) {
                    xmlSerializerNamespaces.Add(kvp.Key, kvp.Value);
                }
            } else { xmlSerializerNamespaces.Add(string.Empty, string.Empty); }

            xmlSerializer.Serialize(stream, graph, xmlSerializerNamespaces);
        }

        #endregion

        #region ISerializer Members

        public byte[] Serialize(object? graph, SerializationOptions? options = null) {
            if (graph == null) { throw new ArgumentNullException(nameof(graph)); }

            var opts = options as XmlSerializationOptions ?? new();

            using var memoryStream = new MemoryStream();
            Serialize(memoryStream, graph, opts.Namespaces);
            memoryStream.Seek(offset: 0, loc: SeekOrigin.Begin);
            return memoryStream.ToArray();
        }

        public object? Deserialize(Type? type, byte[]? buffer, SerializationOptions? options = null) {
            if (buffer == null) { throw new ArgumentNullException(nameof(buffer)); }

            using var memoryStream = new MemoryStream(buffer);
            return Deserialize(type, memoryStream);
        }

        public void Serialize(Stream? stream, object? graph, SerializationOptions? options = null) {
            if (stream == null) { throw new ArgumentNullException(nameof(stream)); }
            if (graph == null) { throw new ArgumentNullException(nameof(graph)); }

            var opts = options as XmlSerializationOptions ?? new();

            Serialize(stream, graph, opts.Namespaces);
        }

        public object? Deserialize(Type? type, Stream? stream, SerializationOptions? options = null) {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }
            if (stream == null) { throw new ArgumentNullException(nameof(stream)); }

            return new MS_XmlSerializer(type).Deserialize(stream);
        }

        #endregion
    }
}