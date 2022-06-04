using Autofac;
using Nameless.DependencyInjection.Autofac;
using Nameless.Services;
using Nameless.WebApplication.Web.Services;

namespace Nameless.WebApplication.Web.Infrastructure {

    public sealed class AppModule : ModuleBase {

        #region Protected Override Methods

        protected override void Load(ContainerBuilder builder) {
            builder
                .RegisterType<AuthService>()
                .As<IAuthService>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<CacheService>()
                .As<ICacheService>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<JWTService>()
                .As<IJwtService>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<RefreshTokenService>()
                .As<IRefreshTokenService>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<UserService>()
                .As<IUserService>()
                .InstancePerLifetimeScope();

            builder
                .RegisterInstance(SystemClock.Instance)
                .As<IClock>()
                .SingleInstance();

            base.Load(builder);
        }

        #endregion
    }
}
