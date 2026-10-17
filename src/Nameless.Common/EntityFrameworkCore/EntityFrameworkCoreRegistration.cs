using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Nameless.Registration;

namespace Nameless.EntityFrameworkCore;

/// <summary>
///     Configuration object used to register Entity Framework Core interceptors
///     and database seeders.
/// </summary>
public class EntityFrameworkCoreRegistration : AssemblyScanAware<EntityFrameworkCoreRegistration> {
    private readonly List<Type> _interceptors = [];
    private readonly List<Type> _databaseSeeders = [];

    /// <summary>
    ///     Whether it should register the Database Context Factory instead
    ///     of the Database Context. Useful in situations where the dependency
    ///     injection scope is not aligned with the context lifetime.
    /// </summary>
    public bool UseDbContextFactory { get; private set; }

    /// <summary>
    ///     Gets or sets an optional override for the default
    ///     <see cref="DbContextOptionsBuilder"/> configuration.
    ///     When set, replaces the default Sqlite configuration.
    /// </summary>
    public Action<IServiceProvider, DbContextOptionsBuilder>? DbContextConfiguration { get; private set; }

    /// <summary>
    ///     Gets the registered EF Core interceptor types.
    /// </summary>
    /// <remarks>
    ///     Important: Interceptors are not registered using assembly scan
    ///     feature. The order in which they are registered is crucial for
    ///     EF Core, since it executes each interceptor in the order they
    ///     were configured.
    /// </remarks>
    public IReadOnlyCollection<Type> Interceptors => _interceptors;

    /// <summary>
    ///     Gets the registered database seeder type, if any.
    /// </summary>
    public IReadOnlyCollection<Type> DatabaseSeeders => UseAssemblyScan
        ? ExecuteAssemblyScan(typeof(IDatabaseSeeder))
        : _databaseSeeders;

    /// <summary>
    ///     Sets whether it should configure
    ///     <see cref="IDbContextFactory{TContext}"/> instead of a single
    ///     instance of <see cref="DbContext"/>.
    /// </summary>
    /// <param name="value">
    ///     The value.
    /// </param>
    /// <returns>
    ///     The current <see cref="EntityFrameworkCoreRegistration"/> so other
    ///     actions can be chained.
    /// </returns>
    public EntityFrameworkCoreRegistration WithUseDbContextFactory(bool value) {
        UseDbContextFactory = value;

        return this;
    }

    /// <summary>
    ///     Sets the delegate to use when configuring the database context.
    /// </summary>
    /// <remarks>
    ///     Database context has a default configuration action, for SQLite.
    ///     When providing a new value, it overrides the entire configuration
    ///     action.
    /// </remarks>
    /// <param name="value">
    ///     The value.
    /// </param>
    /// <returns>
    ///     The current <see cref="EntityFrameworkCoreRegistration"/> so other
    ///     actions can be chained.
    /// </returns>
    public EntityFrameworkCoreRegistration WithDbContextConfiguration(Action<IServiceProvider, DbContextOptionsBuilder> value) {
        DbContextConfiguration = value;

        return this;
    }

    /// <summary>
    ///     Registers an EF Core interceptor by type parameter.
    /// </summary>
    /// <typeparam name="TInterceptor">
    ///     The interceptor type to register.
    /// </typeparam>
    /// <returns>
    ///     The current <see cref="EntityFrameworkCoreRegistration"/> so other
    ///     actions can be chained.
    /// </returns>
    public EntityFrameworkCoreRegistration WithInterceptor<TInterceptor>() where TInterceptor : IInterceptor {
        return WithInterceptor(typeof(TInterceptor));
    }

    /// <summary>
    ///     Registers an EF Core interceptor by its <see cref="Type"/>.
    /// </summary>
    /// <param name="type">
    ///     The concrete interceptor type to register.
    /// </param>
    /// <returns>
    ///     The current <see cref="EntityFrameworkCoreRegistration"/> so other
    ///     actions can be chained.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     if <paramref name="type"/> is not assignable from
    ///     <see cref="IInterceptor"/>, is an open generic type, or is a
    ///     non-concrete type.
    /// </exception>
    public EntityFrameworkCoreRegistration WithInterceptor(Type type) {
        Throws.When.IsNotAssignableFrom(type, typeof(IInterceptor));
        Throws.When.IsOpenGenericType(type);
        Throws.When.IsNonConcreteType(type);

        // Interceptors must be registered as list objects since
        // they need to ensure order of execution.
        if (!_interceptors.Contains(type)) {
            _interceptors.Add(type);
        }

        return this;
    }

    /// <summary>
    ///     Registers a database seeder by type parameter.
    /// </summary>
    /// <typeparam name="TDatabaseSeeder">
    ///     The seeder type to register.
    /// </typeparam>
    /// <returns>
    ///     The current <see cref="EntityFrameworkCoreRegistration"/> so other
    ///     actions can be chained.
    /// </returns>
    public EntityFrameworkCoreRegistration WithDatabaseSeeder<TDatabaseSeeder>() where TDatabaseSeeder : IDatabaseSeeder {
        return WithDatabaseSeeder(typeof(TDatabaseSeeder));
    }

    /// <summary>
    ///     Registers a database seeder by its <see cref="Type"/>.
    /// </summary>
    /// <param name="type">
    ///     The concrete seeder type to register.
    /// </param>
    /// <returns>
    ///     The current <see cref="EntityFrameworkCoreRegistration"/> so other
    ///     actions can be chained.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     if <paramref name="type"/> is not assignable from
    ///     <see cref="IDatabaseSeeder"/>, is an open generic type, or is a
    ///     non-concrete type.
    /// </exception>
    public EntityFrameworkCoreRegistration WithDatabaseSeeder(Type type) {
        Throws.When.IsNotAssignableFrom(type, typeof(IDatabaseSeeder));
        Throws.When.IsOpenGenericType(type);
        Throws.When.IsNonConcreteType(type);

        if (!_databaseSeeders.Contains(type)) {
            _databaseSeeders.Add(type);
        }

        return this;
    }
}