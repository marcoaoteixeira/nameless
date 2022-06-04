using System.Reflection;
using Nameless.Helpers;

namespace Nameless.Persistence {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class IDAttribute : Attribute {

        #region Public Static Read-Only Properties

        /// <summary>
        /// Gets the convention fields for identifiers.
        /// Values are "_id" and "id".
        /// </summary>
        public static string[] ConventionFields => new[] { "_id", "id" };
        /// <summary>
        /// Gets the convention properties for identifiers.
        /// Values are "Id" and "ID".
        /// </summary>
        public static string[] ConventionProperties => new[] { "Id", "ID" };

        #endregion

        #region Public Static Methods

        public static ID GetID<T>() => GetID(typeof(T), null);

        public static ID GetID<T>(T instance) => GetID(typeof(T), instance);

        public static ID GetID(Type type, object? instance) {
            Prevent.Null(type, nameof(type));

            var field = type
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(_ => ConventionFields.Contains(_.Name) || _.GetCustomAttribute<IDAttribute>() != null);

            if (field != null) {
                return new ID(field.Name, instance != null ? field.GetValue(instance) : null);
            }

            var property = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(_ => ConventionProperties.Contains(_.Name) || _.GetCustomAttribute<IDAttribute>() != null);

            if (property != null) {
                return new ID(property.Name, instance != null ? property.GetValue(instance) : null);
            }

            return ID.Null;
        }

        #endregion
    }

    public sealed class ID {

        #region Public Static Read-Only Fields

        public static readonly ID Null = new(null, null);

        #endregion

        #region Private Read-Only Fields

        private readonly string? _name;
        private readonly object? _value;

        #endregion

        #region Public Properties

        public string? Name => _name;
        public object? Value => _value;

        #endregion

        #region Public Constructors

        public ID(string? name, object? value) {
            _name = name;
            _value = value;
        }

        #endregion

        #region Public Methods

        public T As<T>() => IDHelper.TryGetAs<T>(_value, out var output) ? output : default!;

        #endregion
    }
}
