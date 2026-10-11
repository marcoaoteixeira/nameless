using System.Reflection;

namespace Nameless;

/// <summary>
///     <see cref="Assembly" /> extension methods.
/// </summary>
public static class AssemblyExtensions {
    /// <param name="self">
    ///     The current assembly.
    /// </param>
    extension(Assembly self) {
        /// <summary>
        ///     Gets the last write time UTC for the given assembly.
        /// </summary>
        /// <remarks>
        ///     If value is <see cref="DateTimeOffset.MinValue"/>, it means
        ///     that the assembly is dynamic or single-file.
        /// </remarks>
        public DateTimeOffset LastWriteTimeUtc => !string.IsNullOrWhiteSpace(self.Location) && SysFile.Exists(self.Location)
            ? SysFile.GetLastWriteTimeUtc(self.Location)
            : DateTimeOffset.MinValue;

        /// <summary>
        ///     Retrieves the assembly directory path.
        /// </summary>
        public string GetDirectoryPath() {
            if (string.IsNullOrWhiteSpace(self.Location)) { return string.Empty; }

            var location = $"file://{self.Location}";
            var uri = new UriBuilder(location);
            var filePath = Uri.UnescapeDataString(uri.Path);

            return SysPath.GetDirectoryName(filePath) ?? string.Empty;
        }

        /// <summary>
        ///     Retrieves the semantic name of the assembly.
        /// </summary>
        /// <returns>
        ///     A string containing the semantic name of the assembly.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the assembly does not have a name.
        /// </exception>
        /// <remarks>
        ///     Under normal circumstances, this exception should never be
        ///     thrown, as every assembly is expected to have a name.
        /// </remarks>
        public string GetSemanticName() {
            return self.GetName().Name
                   ?? throw new InvalidOperationException("Assembly has no name.");
        }

        /// <summary>
        ///     Retrieves the semantic version for the current assembly.
        ///     See <a href="https://semver.org/">Semantic Versioning</a>.
        /// </summary>
        /// <param name="prefix">
        ///     The prefix.
        /// </param>
        /// <returns>
        ///     A <see cref="string"/> representing the semantic version.
        /// </returns>
        public string GetSemanticVersion(char? prefix = null) {
            var version = self.GetName().Version ??
                          new Version(major: 0, minor: 0, build: 0);

            return version.ToSemVersion(prefix).Format(includePrefix: prefix is not null);
        }

        /// <summary>
        ///     Searches for all concrete implementations of a given service
        ///     <paramref name="service" />.
        /// </summary>
        /// <param name="service">
        ///     The service type.
        /// </param>
        /// <returns>
        ///     A collection of types that implements <paramref name="service" />.
        /// </returns>
        /// <remarks>
        ///     We look for all types in the assembly that are not pointer
        ///     types, not by-ref types, not abstract, and are visible outside
        ///     the assembly (public, or public and nested in public types).
        ///     Then we check if the type is assignable to the service type or
        ///     if it is a generic type that is assignable to the service type.
        /// </remarks>
        public IEnumerable<Type> GetImplementations(Type service) {
            // retrieve all types from the assembly that are relevant to us
            return self.GetTypes()
                       .Where(type => type is {
                           // Exclude pointer types
                           IsPointer: false,

                           // Exclude by-ref types
                           IsByRef: false,

                           // Only types visible outside the assembly,
                           // including public types nested in public types
                           IsVisible: true,

                           // Concrete types that can be instantiated
                           IsClass: true,

                           // Exclude abstract types (can't be instantiated)
                           IsAbstract: false,

                           // Exclude interface types (can't be instantiated)
                           IsInterface: false
                       })
                       // inside the types, we will look for all types that
                       // are assignable to the service type
                       .Where(type => service.IsAssignableFrom(type) ||
                                      service.IsAssignableFromGeneric(type));
        }
    }

    /// <param name="self">
    ///     The current collection of <see cref="Assembly"/>.
    /// </param>
    extension(IEnumerable<Assembly> self) {
        /// <summary>
        ///     Searches for all concrete implementations of all given services
        ///     <paramref name="services" />.
        /// </summary>
        /// <param name="services">
        ///     The service type.
        /// </param>
        /// <returns>
        ///     A collection of types that implements any of
        ///     <paramref name="services" />.
        /// </returns>
        public IEnumerable<Type> GetImplementations(IEnumerable<Type> services) {
            return
                from assembly in self
                from service in services
                from implementation in assembly.GetImplementations(service)
                select implementation;
        }
    }
}