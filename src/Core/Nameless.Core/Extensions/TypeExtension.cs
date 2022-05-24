using System.Reflection;

namespace Nameless {

    /// <summary>
    /// Extension methods for <see cref="Type"/>.
    /// </summary>
    public static class TypeExtension {

        #region Private Static Read-Only Fields

        private static readonly Type[] WriteTypes = new[] {
            typeof (string),
            typeof (DateTime),
            typeof (Enum),
            typeof (decimal),
            typeof (Guid)
        };

        #endregion Private Static Read-Only Fields

        #region Public Static Methods

        /// <summary>
        /// Determines whether the <paramref name="genericType"/> is assignable from
        /// <paramref name="self"/> taking into account generic definitions
        /// </summary>
        /// <param name="self">The given type.</param>
        /// <param name="genericType">The generic type.</param>
        /// <returns><c>true</c> if <paramref name="genericType"/> is assignable from <paramref name="self"/>, otherwise, <c>false</c>.</returns>
        public static bool IsAssignableToGenericType(this Type? self, Type genericType) {
            if (self == null || genericType == null) { return false; }

            return self == genericType ||
                MapsToGenericTypeDefinition(self, genericType) ||
                HasInterfaceThatMapsToGenericTypeDefinition(self, genericType) ||
                self.BaseType.IsAssignableToGenericType(genericType);
        }

        /// <summary>
        /// Verifies if the <see cref="Type"/> is an instance of <see cref="Nullable"/>.
        /// </summary>
        /// <param name="self">The self type.</param>
        /// <returns><c>true</c>, if is instance of <see cref="Nullable"/>, otherwise, <c>false</c>.</returns>
        public static bool IsNullable(this Type self) {
            return self != null && self.IsGenericType && self.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Can convert to <see cref="Nullable"/> type.
        /// </summary>
        /// <param name="self">The self type.</param>
        /// <returns><c>true</c>, if can convert, otherwise, <c>false</c>.</returns>
        public static bool AllowNull(this Type self) => self != null && (!self.IsValueType || self.IsNullable());

        /// <summary>
        /// Retrieves the generic method associated to the self type.
        /// </summary>
        /// <param name="self">The self type.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="genericArgumentTypes">Method generic argument types, if any.</param>
        /// <param name="argumentTypes">Method argument types, if any.</param>
        /// <param name="returnType">Method return type.</param>
        /// <returns>Returns an instance of <see cref="MethodInfo"/> representing the generic method.</returns>
        public static MethodInfo? GetGenericMethod(this Type self, string name, Type[] genericArgumentTypes, Type[] argumentTypes, Type returnType) {
            if (self == null) { return null; }

            Ensure.NotNullEmptyOrWhiteSpace(name, nameof(name));
            Ensure.NotNull(genericArgumentTypes, nameof(genericArgumentTypes));
            Ensure.NotNull(argumentTypes, nameof(argumentTypes));
            Ensure.NotNull(returnType, nameof(returnType));

            return self.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(method =>
                   method.Name == name &&
                   method.GetGenericArguments().Length == genericArgumentTypes.Length &&
                   method.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(argumentTypes) &&
                   (method.ReturnType.IsGenericType && !method.ReturnType.IsGenericTypeDefinition ? returnType.GetGenericTypeDefinition() : method.ReturnType) == returnType)
                .Single()
                .MakeGenericMethod(genericArgumentTypes);
        }

        /// <summary>
        /// Returns a value that indicates whether the current type can be assigned to the
        /// specified type.
        /// </summary>
        /// <param name="self">The current type.</param>
        /// <param name="type">The specified type.</param>
        /// <returns>
        /// <c>true</c> if the current type can be assigned to the specified type;
        /// otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAssignableTo(this Type self, Type type) {
            if (self == null) { return false; }

            Ensure.NotNull(type, nameof(type));

            return type.IsAssignableFrom(self);
        }

        /// <summary>
        /// Returns a value that indicates whether the current type can be assigned to the
        /// specified type.
        /// </summary>
        /// <typeparam name="T">The specified type.</typeparam>
        /// <param name="self">The current type.</param>
        /// <returns>
        /// <c>true</c> if the current type can be assigned to the specified type;
        /// otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAssignableTo<T>(this Type self) => IsAssignableTo(self, typeof(T));

        /// <summary>
        /// Verifies if the <paramref name="self"/> is a simple type.
        /// </summary>
        /// <param name="self">The self type.</param>
        /// <returns><c>true</c> if is simple type; otherwise, <c>false</c>.</returns>
        public static bool IsSimple(this Type self) {
            return self != null && (self.IsPrimitive || WriteTypes.Contains(self));
        }

        /// <summary>
        /// Retrieves the first occurence of the specified generic argument.
        /// </summary>
        /// <param name="self">The source type.</param>
        /// <param name="genericArgumentType">The generic argument type.</param>
        /// <returns>The generic argument type, if found.</returns>
        public static Type? GetFirstOccurenceOfGenericArgument(this Type? self, Type genericArgumentType) {
            Ensure.NotNull(genericArgumentType, nameof(genericArgumentType));

            if (self == null) { return null; }

            var args = self.GetGenericArguments();
            var result = args.FirstOrDefault(arg => arg.IsAssignableToGenericType(genericArgumentType));
            return result ?? self.BaseType.GetFirstOccurenceOfGenericArgument(genericArgumentType);
        }

        #endregion Public Static Methods

        #region Private Static Methods

        private static bool MapsToGenericTypeDefinition(Type self, Type genericType) => genericType.IsGenericTypeDefinition &&
            self.IsGenericType &&
            self.GetGenericTypeDefinition() == genericType;

        private static bool HasInterfaceThatMapsToGenericTypeDefinition(Type self, Type genericType) => self
            .GetInterfaces()
            .Where(_ => _.IsGenericType)
            .Any(_ => _.GetGenericTypeDefinition() == genericType);

        #endregion Private Static Methods
    }
}