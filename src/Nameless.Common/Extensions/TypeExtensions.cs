using System.Text;

namespace Nameless;

/// <summary>
///     <see cref="Type" /> extension methods.
/// </summary>
public static class TypeExtensions {
    /// <param name="self">
    ///     The current <see cref="Type" /> instance.
    /// </param>
    extension(Type self) {
        /// <summary>
        ///     Whether the type is an open generic, meaning that it is
        ///     a generic type definition or it has generic parameters.
        /// </summary>
        /// <returns>
        ///     <see langword="true"/> if the type is an open generic;
        ///     otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsOpenGeneric => self.IsGenericTypeDefinition || self.ContainsGenericParameters;

        /// <summary>
        ///     Checks if the <see cref="Type" /> is a concrete class.
        /// </summary>
        /// <returns>
        ///     <see langword="true"/> if the type is a concrete class;
        ///     otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsConcrete => self is {
            IsClass: true,
            IsAbstract: false,
            IsInterface: false
        };

        /// <summary>
        ///     Verifies if the <see cref="Type" /> is an instance of <see cref="Nullable" />.
        /// </summary>
        /// <returns><see langword="true"/>, if is instance of <see cref="Nullable" />, otherwise, <see langword="false"/>.</returns>
        public bool IsNullable => self.IsGenericType && self.GetGenericTypeDefinition() == typeof(Nullable<>);

        /// <summary>
        ///     Can convert to <see cref="Nullable" /> type.
        /// </summary>
        /// <returns>
        ///     <see langword="true"/>, if it can convert;
        ///     otherwise, <see langword="false"/>.
        /// </returns>
        public bool AllowNull => !self.IsValueType || self.IsNullable;

        /// <summary>
        ///     Gets a simple tag.
        /// </summary>
        /// <returns>
        ///     A string representing the type tag.
        /// </returns>
        public string Tag => self.Name.ToSnakeCase().ToUpperInvariant();

        /// <summary>
        ///     Checks if the current open generic type is assignable from
        ///     the <paramref name="type" />.
        /// </summary>
        /// <param name="type">
        ///     The assignable from type.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if assignable; otherwise
        ///     <see langword="false"/>.
        /// </returns>
        public bool IsAssignableFromGeneric(Type? type) {
            if (!self.IsGenericType) { return false; }

            while (true) {
                if (type is null) { return false; }

                // Check if any interface has the same open generic definition.
                var assignable = type.GetInterfaces()
                                     .Any(Assignable);
                if (assignable) { return true; }

                // Check if any class has the same open generic definition.
                if (Assignable(type)) { return true; }

                type = type.BaseType;
            }

            bool Assignable(Type current) {
                return current.IsGenericType &&
                       current.GetGenericTypeDefinition() == self.GetGenericTypeDefinition();
            }
        }

        /// <summary>
        ///     Checks if the current type implements the specified interface.
        /// </summary>
        /// <typeparam name="TInterface">Type of the interface.</typeparam>
        /// <returns><see langword="true"/> if implements; otherwise <see langword="false"/>.</returns>
        public bool HasInterface<TInterface>()
            where TInterface : class {
            return self.HasInterface(typeof(TInterface));
        }

        /// <summary>
        ///     Checks if the current type implements the specified interface defined by <paramref name="interfaceType" />.
        /// </summary>
        /// <param name="interfaceType">The interface type.</param>
        /// <returns><see langword="true"/> if implements; otherwise <see langword="false"/>.</returns>
        public bool HasInterface(Type interfaceType) {
            return self.GetInterfaces()
                       .Any(type => interfaceType.IsAssignableFrom(type) ||
                                    interfaceType.IsAssignableFromGeneric(type));
        }

        /// <summary>
        ///     Checks if type has the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">Type of the attribute.</typeparam>
        /// <param name="inherit">Whether the attribute is inherited.</param>
        /// <returns><see langword="true"/> if the type has the attribute; otherwise <see langword="false"/>.</returns>
        public bool HasAttribute<TAttribute>(bool inherit = false)
            where TAttribute : Attribute {
            return self.HasAttribute(typeof(TAttribute), inherit);
        }

        /// <summary>
        ///     Checks if type has the specified attribute.
        /// </summary>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <param name="inherit">Whether the attribute is inherited.</param>
        /// <returns><see langword="true"/> if the type has the attribute; otherwise <see langword="false"/>.</returns>
        public bool HasAttribute(Type attributeType, bool inherit = false) {
            return self.GetCustomAttributes(attributeType, inherit).Length > 0;
        }

        /// <summary>
        ///     Checks if the type has a parameterless constructor.
        /// </summary>
        /// <returns><see langword="true"/> if the type has a parameterless constructor; otherwise <see langword="false"/>.</returns>
        public bool HasParameterlessConstructor() {
            return self.GetConstructor(Type.EmptyTypes) is not null ||
                   self is { IsValueType: true, IsNullable: false };
        }

        /// <summary>
        ///     Gets the pretty name of the type.
        /// </summary>
        /// <returns>
        ///     The pretty name of the type.
        /// </returns>
        /// <remarks>
        ///     This method is used to get the pretty name of the type, which is
        ///     useful for logging and debugging purposes. When you call the
        ///     property Name of a type and the type is generic, it will return
        ///     something like List`1 instead of List&lt;int&gt;. This method will
        ///     return List&lt;int&gt; instead. Or if the type is just the template
        ///     it will return List&lt;&gt;.
        /// </remarks>
        public string GetPrettyName() {
            // A type nested in a generic type is generic through its
            // declaring type only, so its name carries no arity suffix.
            var aritySeparator = self.Name.IndexOf(value: '`', StringComparison.Ordinal);
            if (!self.IsGenericType || aritySeparator < 0) {
                return self.Name;
            }

            var arguments = self.GetGenericArguments();
            var baseName = self.Name[..aritySeparator];

            var sb = new StringBuilder();
            sb.Append(baseName);
            sb.Append(value: '<');
            for (var index = 0; index < arguments.Length; index++) {
                if (index > 0) { sb.Append(value: ','); }

                // gets the next argument, which can be a generic type
                sb.Append(arguments[index].GetPrettyName());
            }

            sb.Append(value: '>');

            return sb.ToString();
        }

        /// <summary>
        /// Determines whether the specified <see cref="Type"/> can be instantiated.
        /// </summary>
        /// <remarks>
        /// This method checks several conditions to determine if a type can be
        /// instantiated: it must not be an open generic type, a pointer type,
        /// or a by-ref type; it must not be abstract; it must be public; and it
        /// must have a parameterless constructor.
        /// </remarks>
        /// <returns>
        /// <see langword="true"/> if the type is instantiable; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool CanInstantiate() {
            return self is {
                // Exclude open generic types
                IsGenericTypeDefinition: false,

                // Exclude pointer types
                IsPointer: false,

                // Exclude by-ref types
                IsByRef: false,

                // Exclude abstract types (can't be instantiated)
                IsAbstract: false,

                // Only public types
                IsPublic: true
            } && self.HasParameterlessConstructor();
        }

        /// <summary>
        ///     Retrieves all interfaces/classes that "closes" the
        ///     current <see cref="Type"/> given the generic definition.
        /// </summary>
        /// <param name="genericDefinition">
        ///     The generic definition type.
        /// </param>
        /// <returns>
        ///     A collection of <see cref="Type"/> that closes
        ///     the current <see cref="Type"/>.
        /// </returns>
        public IEnumerable<Type> GetTypesThatCloses(Type genericDefinition) {
            return genericDefinition.IsGenericTypeDefinition
                ? GetTypesThatClosesCore(self, genericDefinition).Distinct()
                : [];
        }

        /// <summary>
        ///     Attempts to resolve a usable <see cref="Type"/> when the
        ///     provided type has an incomplete or non-representable metadata
        ///     identity (e.g. <see cref="Type.FullName"/> is <c>null</c>),
        ///     which commonly occurs with open generics, generic parameters,
        ///     or reflected interface types.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     If no representative type is found.
        /// </exception>
        /// <returns>
        ///     If there is no need to solve the type, it will return the
        ///     current <see cref="Type"/>; otherwise, a semantically
        ///     equivalent <see cref="Type"/> from the original
        ///     <paramref name="self"/>.
        /// </returns>
        /// <remarks>
        ///     This method does not mutate the original <see cref="Type"/>
        ///     instance. Due to runtime and metadata constraints in .NET,
        ///     not all types can be fully resolved or assigned a non-null
        ///     <see cref="Type.FullName"/>. This helper provides a
        ///     best-effort resolution strategy.
        /// </remarks>
        public Type FixTypeReference() {
            if (!string.IsNullOrWhiteSpace(self.FullName)) {
                return self;
            }

            var typeName = self.DeclaringType is not null
                ? $"{self.DeclaringType.FullName}+{self.Name}, {self.Assembly.FullName}"
                : $"{self.Namespace}.{self.Name}, {self.Assembly.FullName}";

            return Type.GetType(typeName, throwOnError: false)
                   ?? throw new InvalidOperationException($"Type '{typeName}' cannot be found.");
        }
    }

    private static IEnumerable<Type> GetTypesThatClosesCore(Type? current, Type genericDefinition) {
        if (current is null || current.IsAbstract || current.IsInterface) { yield break; }

        if (genericDefinition.IsInterface) {
            var interfaces = current.GetInterfaces()
                                    .Where(type => type.IsGenericType &&
                                                   type.GetGenericTypeDefinition() == genericDefinition);
            foreach (var @interface in interfaces) {
                yield return @interface.FixTypeReference();
            }
        }
        else if (current.BaseType is { IsGenericType: true } &&
                 current.BaseType.GetGenericTypeDefinition() == genericDefinition) {
            yield return current.BaseType.FixTypeReference();
        }

        if (current.BaseType == typeof(object)) {
            yield break;
        }

        foreach (var @interface in GetTypesThatClosesCore(current.BaseType, genericDefinition)) {
            yield return @interface.FixTypeReference();
        }
    }
}