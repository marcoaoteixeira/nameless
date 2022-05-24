using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;

namespace Nameless.EventSourcing {

    internal class PrivateReflectionDynamicObject : DynamicObject {

        #region Private Constants

        private const BindingFlags CurrentBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        #endregion

        #region Private Read-Only Fields

        private readonly ConcurrentDictionary<int, CompiledMethodInfo?> Cache = new();

        #endregion

        #region Internal Properties

        internal object RealObject { get; }

        #endregion

        #region Internal Constructors

        internal PrivateReflectionDynamicObject(object realObject) {
            RealObject = realObject;
        }

        #endregion

        #region Public Override Methods

        public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result) {
            Ensure.NotNull(args, nameof(args));

            var methodName = binder.Name;
            var type = RealObject.GetType();

            var hash = 13;
            unchecked {
                hash += type.GetHashCode() * 7;
                hash += methodName.GetHashCode() * 7;
            }

            var argumentTypes = new Type?[args!.Length];
            for (var idx = 0; idx < args.Length; idx++) {
                var argumentType = args[idx]?.GetType();
                argumentTypes[idx] = argumentType;
                if (argumentType != null) {
                    unchecked {
                        hash += argumentType.GetHashCode() * 7;
                    }
                }
            }
            var method = Cache.GetOrAdd(hash, _ => {
                var member = GetMember(type, methodName, argumentTypes);
                return member != null ? new CompiledMethodInfo(member, type) : default;
            });
            result = method?.Invoke(RealObject, args!);

            return true;
        }

        #endregion

        #region Private Static Methods

        private static MethodInfo? GetMember(Type? type, string name, Type?[] argumentTypes) {
            if (type == null) { return null; }

            var member = type
                .GetMethods(CurrentBindingFlags)
                .FirstOrDefault(method => method.Name == name && method.GetParameters()
                                                                       .Select(parameter => parameter.ParameterType)
                                                                       .SequenceEqual(argumentTypes));

            if (member != null) { return member; }

            return GetMember(type.BaseType, name, argumentTypes);
        }

        #endregion
    }
}
