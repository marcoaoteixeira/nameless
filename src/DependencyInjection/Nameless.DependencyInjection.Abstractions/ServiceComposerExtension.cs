namespace Nameless.DependencyInjection {

    public static class ServiceComposerExtension {

        #region Public Static Methods

        public static IServiceComposer AddTransient<TService, TImplemetation>(this IServiceComposer self, params Parameter[] parameters) {
            if (self == null) { throw new ArgumentNullException(nameof(self)); }

            return self.AddTransient(typeof(TService), typeof(TImplemetation), parameters);
        }

        public static IServiceComposer AddPerScope<TService, TImplemetation>(this IServiceComposer self, params Parameter[] parameters) {
            if (self == null) { throw new ArgumentNullException(nameof(self)); }

            return self.AddPerScope(typeof(TService), typeof(TImplemetation), parameters);
        }

        public static IServiceComposer AddSingleton<TService, TImplemetation>(this IServiceComposer self, params Parameter[] parameters) {
            if (self == null) { throw new ArgumentNullException(nameof(self)); }

            return self.AddSingleton(typeof(TService), typeof(TImplemetation), parameters);
        }

        public static IServiceComposer AddModule<TModule>(this IServiceComposer self) where TModule : IModule {
            if (self == null) { throw new ArgumentNullException(nameof(self)); }

            return self.AddModule(typeof(TModule));
        }

        #endregion
    }
}