using System.Reflection;

namespace Nameless.DependencyInjection.Autofac {

    public static class Utils {

		#region Public Static Methods

		public static PropertyInfo[] FindPropertiesToInject(Type type, Type propertyType) {
			// Look for settable properties
			var result = type
				.GetProperties(BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance)
				.Select(property => new {
					PropertyInfo = property,
					property.PropertyType,
					IndexParameters = property.GetIndexParameters().ToArray(),
					property.CanWrite
				})
				.Where(property => property.PropertyType == propertyType) // must be a logger
				.Where(property => property.CanWrite) // must be writable
				.Where(property => property.IndexParameters.Length == 0); // must not be an indexer

			return result.Select(_ => _.PropertyInfo).ToArray();
		}

		#endregion
	}
}
