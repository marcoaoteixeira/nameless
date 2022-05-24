using System.Text;
using Newtonsoft.Json;

namespace Nameless.Serialization.Json {

    public sealed class JsonSerializationOptions : SerializationOptions {

        #region Public Static Read-Only Properties

        public static JsonSerializationOptions Default => new();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the encoding. Default is <see cref="Encoding.UTF8" />
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        /// <summary>
        /// Gets or sets the JSON serializer settings.
        /// </summary>
        public JsonSerializerSettings Settings { get; set; } = new();

        #endregion
    }
}