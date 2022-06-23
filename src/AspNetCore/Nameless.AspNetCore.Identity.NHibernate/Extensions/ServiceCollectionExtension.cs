using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Nameless.AspNetCore.Identity.NHibernate {

    public static class ServiceCollectionExtension {

        #region Public Static Methods

        public static IdentityBuilder AddIdentityStores<TIdentityContext>(this IdentityBuilder builder)
            where TIdentityContext : IIdentityContext {
            var services = builder.Services;
            var userType = builder.UserType;
            var roleType = builder.RoleType;
            var contextType = typeof(TIdentityContext);


            var identityUserType = FindGenericBaseType(userType, typeof(IdentityUser<>));
            if (identityUserType == null) {
                throw new InvalidOperationException($"Not an {typeof(IdentityUser<>).Name} implementation.");
            }

            var keyType = identityUserType.GenericTypeArguments[0];

            if (roleType != null) {
                var identityRoleType = FindGenericBaseType(roleType, typeof(IdentityRole<>));
                if (identityRoleType == null) {
                    throw new InvalidOperationException($"Not an {typeof(IdentityRole<>).Name} implementation.");
                }

                Type userStoreType;
                Type roleStoreType;
                var identityContext = FindGenericBaseType(contextType, typeof(IdentityContext<,,,,,,,>));
                if (identityContext == null) {
                    // If its a custom IIdentityContext, we can only add the default POCOs
                    userStoreType = typeof(UserStore<,,>).MakeGenericType(userType, roleType, keyType);
                    roleStoreType = typeof(RoleStore<,>).MakeGenericType(roleType, keyType);
                } else {
                    userStoreType = typeof(UserStore<,,,,,,,>).MakeGenericType(userType, roleType,
                        identityContext.GenericTypeArguments[2],
                        identityContext.GenericTypeArguments[3],
                        identityContext.GenericTypeArguments[4],
                        identityContext.GenericTypeArguments[5],
                        identityContext.GenericTypeArguments[6],
                        identityContext.GenericTypeArguments[7]);
                    roleStoreType = typeof(RoleStore<,,,>).MakeGenericType(roleType,
                        identityContext.GenericTypeArguments[2],
                        identityContext.GenericTypeArguments[4],
                        identityContext.GenericTypeArguments[7]);
                }
                services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
                services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType), roleStoreType);
            }

            if (roleType == null) {
                Type userStoreType;
                var identityContext = FindGenericBaseType(contextType, typeof(IdentityUserOnlyContext<,,,,>));
                if (identityContext == null) {
                    // If its a custom DbContext, we can only add the default POCOs
                    userStoreType = typeof(UserOnlyStore<,>).MakeGenericType(userType, keyType);
                } else {
                    userStoreType = typeof(UserOnlyStore<,,,,>).MakeGenericType(userType,
                        identityContext.GenericTypeArguments[1],
                        identityContext.GenericTypeArguments[2],
                        identityContext.GenericTypeArguments[3],
                        identityContext.GenericTypeArguments[4]);
                }
                services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
            }

            return builder;
        }

        #endregion

        #region Private Static Methods

        private static Type? FindGenericBaseType(Type currentType, Type genericBaseType) {
            var type = currentType;
            while (type != null) {
                var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
                if (genericType != null && genericType == genericBaseType) {
                    return type;
                }
                type = type.BaseType;
            }
            return default;
        }

        #endregion
    }
}
