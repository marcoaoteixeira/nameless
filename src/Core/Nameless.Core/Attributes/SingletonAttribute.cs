using System.Reflection;

namespace Nameless {

    /// <summary>
    /// Classes marked with <see cref="SingletonAttribute"/> must have a static
    /// property, defined by the property <see cref="Accessor"/>, that will
    /// returns the singleton instance of that said class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SingletonAttribute : Attribute {

        #region Public Properties

        /// <summary>
        /// Gets or sets the singleton accessor property name.
        /// </summary>
        /// <remarks>Default is "Instance".</remarks>
        public string? Accessor { get; set; }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Checks if the specified type is a singleton.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <returns><c>true</c> if is a singleton; otherwise <c>false</c>.</returns>
        public static bool IsSingleton<T>() {
            return IsSingleton(typeof(T));
        }

        /// <summary>
        /// Checks if the specified type is a singleton.
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns><c>true</c> if is a singleton; otherwise <c>false</c>.</returns>
        public static bool IsSingleton(Type type) {
            Prevent.Null(type, nameof(type));

            return GetAccessorProperty(type) != null;
        }

        /// <summary>
        /// Retrieves the singleton instance of the type.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <returns>A singleton instance of the type.</returns>
        public static T? GetInstance<T>() where T : class => GetInstance(typeof(T)) as T;

        /// <summary>
        /// Retrieves the singleton instance of the type.
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>A singleton instance of the type.</returns>
        public static object? GetInstance(Type type) {
            Prevent.Null(type, nameof(type));

            var accessorProperty = GetAccessorProperty(type!);
            if (accessorProperty == null) { return default; }

            return accessorProperty.GetValue(obj: null /* static instance */);
        }

        #endregion

        #region Private Static Methods

        private static PropertyInfo? GetAccessorProperty(Type type) {
            var attr = type.GetCustomAttribute<SingletonAttribute>(inherit: false);
            if (attr == null) { return default; }

            var accessorName = attr.Accessor?.OnBlank("Instance");
            return type.GetProperty(accessorName!, BindingFlags.Public | BindingFlags.Static);
        }

        #endregion
    }
}
