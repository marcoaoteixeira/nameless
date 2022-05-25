using System.Reflection;

namespace Nameless.Helpers {

    /// <summary>
    /// Reflection helper.
    /// </summary>
    public static class ReflectionHelper {

        #region Public Static Methods

        /// <summary>
        /// Searchs, recursively, and retrieves the value of a private read-only field.
        /// </summary>
        /// <param name="instance">The <see cref="object"/> instance where the field belongs.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        public static object? GetPrivateFieldValue(object instance, string name) {
            Prevent.Null(instance, nameof(instance));
            Prevent.NullEmptyOrWhiteSpace(name, nameof(name));

            var field = GetPrivateField(instance.GetType(), name);

            if (field == null) {
                throw new FieldAccessException($"Field \"{name}\" not found.");
            }

            return field.GetValue(instance);
        }

        /// <summary>
        /// Searchs, recursively, and sets the value of a private read-only field.
        /// </summary>
        /// <param name="instance">The <see cref="object"/> instance where the field belongs.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The new value.</param>
        public static void SetPrivateFieldValue(object instance, string name, object value) {
            Prevent.Null(instance, nameof(instance));
            Prevent.NullEmptyOrWhiteSpace(name, nameof(name));

            var field = GetPrivateField(instance.GetType(), name);

            if (field == null) {
                throw new FieldAccessException($"Field \"{name}\" not found.");
            }

            field.SetValue(instance, value);
        }

        /// <summary>
        /// Retrieves all methods by a specific signature.
        /// </summary>
        /// <param name="type">The type that will be looked up.</param>
        /// <param name="returnType">The method return type.</param>
        /// <param name="methodAttributeType">The method attribute type, if exists.</param>
        /// <param name="matchParameterInheritance">If will match parameter inheritance.</param>
        /// <param name="parameterTypes">The method parameters type.</param>
        /// <returns>An <see cref="IEnumerable{MethodInfo}"/> with all found methods.</returns>
        public static IEnumerable<MethodInfo> GetMethodsBySignature(Type type, Type? returnType = null, Type? methodAttributeType = null, bool matchParameterInheritance = true, params Type[] parameterTypes) {
            Prevent.Null(type, nameof(type));

            return type.GetRuntimeMethods().Where(method => {
                var currentReturnType = returnType ?? typeof(void);
                if (method.ReturnType != currentReturnType) { return false; }

                if (methodAttributeType != null && !method.GetCustomAttributes(methodAttributeType, inherit: true).Any()) {
                    return false;
                }

                var parameters = method.GetParameters();
                var currentParameterTypes = (parameterTypes ?? Enumerable.Empty<Type>()).ToArray();
                if (parameters.Length != currentParameterTypes.Length) { return false; }

                return currentParameterTypes.Select((parameterType, index) => {
                    var match = parameters[index].ParameterType == parameterType;
                    var assignable = parameterType.IsAssignableFrom(parameters[index].ParameterType);

                    return match || (assignable && matchParameterInheritance);
                }).All(result => result == true);
            });
        }

        #endregion

        #region Private Static Methods

        private static FieldInfo? GetPrivateField(Type type, string name) {
            var result = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);

            if (result == null && type.BaseType != null) {
                return GetPrivateField(type.BaseType, name);
            }

            return result;
        }

        #endregion
    }
}