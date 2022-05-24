using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nameless.AspNetCore.Extensions {

    public static class ServiceCollectionExtension {

        #region Public Static Methods

        public static TOptions? ConfigureOptions<TOptions>(this IServiceCollection self, IConfiguration configuration, Func<TOptions> optionsProvider) where TOptions : class {
            if (self == null) { return default; }

            Ensure.NotNull(configuration, nameof(configuration));
            Ensure.NotNull(optionsProvider, nameof(optionsProvider));

            var options = optionsProvider();
            configuration.Bind(options);
            self.AddSingleton(options);
            return options;
        }

        public static TOptions? ConfigureOptions<TOptions>(this IServiceCollection self, IConfiguration configuration) where TOptions : class, new() {
            if (self == null) { return default; }

            Ensure.NotNull(configuration, nameof(configuration));

            var options = new TOptions();
            configuration.Bind(options);
            self.AddSingleton(options);
            return options;
        }

        #endregion
    }
}
