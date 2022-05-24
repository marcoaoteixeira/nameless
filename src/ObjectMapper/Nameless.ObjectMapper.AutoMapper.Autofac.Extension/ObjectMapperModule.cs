using Autofac;
using Autofac.Core;
using AutoMapper;
using Nameless.DependencyInjection.Autofac;

namespace Nameless.ObjectMapper.AutoMapper {

    public sealed class ObjectMapperModule : ModuleBase {

        #region Public Properties

        public Type[] Profiles { get; set; } = Array.Empty<Type>();

        #endregion

        #region Protected Override Methods

        protected override void Load(ContainerBuilder builder) {
            builder
                .RegisterType<Mapper>()
                .As<IMapper>()
                .OnPreparing(OnPreparingMapper)
                .SetLifetimeScope(LifetimeScopeType.Singleton);

            base.Load(builder);
        }

        #endregion

        #region Private Methods

        private void OnPreparingMapper(PreparingEventArgs args) {
            if (Profiles.IsNullOrEmpty()) { return; }

            var profiles = Profiles
                .Where(_ => typeof(Profile).IsAssignableFrom(_))
                .Select(_ => Activator.CreateInstance(_) as Profile)
                .Where(_ => _ != null)
                .ToArray();

            args.Parameters = args.Parameters.Union(new[] {
                new NamedParameter(nameof(profiles), profiles)
            });
        }

        #endregion
    }
}
