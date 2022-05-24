using System.Collections.Generic;

namespace Nameless.Serialization.Xml {

	public sealed class XmlSerializationOptions : SerializationOptions {

		#region Public Properties

		public IDictionary<string, string> Namespaces { get; } = new Dictionary<string, string>();

		#endregion
	}
}