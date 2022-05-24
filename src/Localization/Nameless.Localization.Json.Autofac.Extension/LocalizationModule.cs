using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Nameless.DependencyInjection.Autofac;

namespace Nameless.Localization.Json {

    public sealed class LocalizationModule : ModuleBase {

        #region Private Constants

        private const string TRANSLATION_PROVIDER_KEY = "66faa31b-c30b-4efc-9e0c-016b5c8a1e17";
        private const string PLURALIZATION_RULE_PROVIDER_KEY = "7dbbb454-e130-4b5a-8891-285596c51bd7";
        private const string CULTURE_CONTEXT_KEY = "5bd5f404-df4b-46d9-9ad4-298b765c5949";

        #endregion

        #region Public Properties

        public Type? CultureContextImplementation { get; set; }
        public Type? PluralizationRuleProviderImplementation { get; set; }

        #endregion

        #region Protected Override Methods

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {
            builder
                .Register<ITranslationProvider, TranslationProvider>(
                    name: TRANSLATION_PROVIDER_KEY,
                    lifetimeScope: LifetimeScopeType.Singleton
                );

            builder
                .Register(
                    service: typeof(IPluralizationRuleProvider),
                    implementation: PluralizationRuleProviderImplementation ?? typeof(DefaultPluralizationRuleProvider),
                    name: PLURALIZATION_RULE_PROVIDER_KEY,
                    lifetimeScope: LifetimeScopeType.Singleton
                );

            builder
                .Register(
                    service: typeof(ICultureContext),
                    implementation: CultureContextImplementation ?? typeof(DefaultCultureContext),
                    name: CULTURE_CONTEXT_KEY,
                    lifetimeScope: LifetimeScopeType.Singleton
                );

            builder
                .Register<IStringLocalizerFactory, StringLocalizerFactory>(
                    lifetimeScope: LifetimeScopeType.Singleton,
                    parameters: new[] {
                        ResolvedParameter.ForNamed<ICultureContext>(CULTURE_CONTEXT_KEY),
                        ResolvedParameter.ForNamed<IPluralizationRuleProvider>(PLURALIZATION_RULE_PROVIDER_KEY),
                        ResolvedParameter.ForNamed<ITranslationProvider>(TRANSLATION_PROVIDER_KEY)
                    }
                );

            base.Load(builder);
        }

        /// <inheritdoc/>
        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration) {
            registration.PipelineBuilding += (sender, pipeline) => {
                pipeline.Use(new FactoryResolveMiddleware(
                    injectType: typeof(IStringLocalizer),
                    factory: ResolveLocalizerDelegate
                ));
            };

            base.AttachToComponentRegistration(componentRegistry, registration);
        }

        #endregion

        #region Private Static Methods

        private static IStringLocalizer ResolveLocalizerDelegate(MemberInfo member, IComponentContext context) => context.Resolve<IStringLocalizerFactory>().Create(member.DeclaringType!);

        #endregion
    }
}
