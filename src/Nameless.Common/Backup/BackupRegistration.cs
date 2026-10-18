using Nameless.Registration;

namespace Nameless.Backup;

/// <summary>
///     Backup &amp; restore registration settings.
/// </summary>
public sealed class BackupRegistration : AssemblyScanAware<BackupRegistration> {
    private readonly HashSet<Type> _routines = [];

    /// <summary>
    ///     Gets the registered backup routines.
    /// </summary>
    /// <remarks>
    ///     When using assembly scan, only the scanned types are returned and
    ///     the ones registered manually are ignored; otherwise, only the ones
    ///     registered manually are returned. Each access executes the scan
    ///     again.
    /// </remarks>
    public IReadOnlyCollection<Type> Routines => UseAssemblyScan
        ? ExecuteAssemblyScan(typeof(IBackupRoutine))
        : _routines;

    /// <summary>
    ///     Registers a backup routine.
    /// </summary>
    /// <typeparam name="TRoutine">
    ///     The routine type.
    /// </typeparam>
    /// <returns>
    ///     The current <see cref="BackupRegistration"/> instance so other
    ///     actions can be chained.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     if <typeparamref name="TRoutine"/> is not a concrete class.
    /// </exception>
    /// <remarks>
    ///     Manually registered routines are ignored when
    ///     <see cref="AssemblyScanAware{TSelf}.UseAssemblyScan"/> is
    ///     <see langword="true"/>.
    /// </remarks>
    public BackupRegistration WithRoutine<TRoutine>() where TRoutine : IBackupRoutine {
        return WithRoutine(typeof(TRoutine));
    }

    /// <summary>
    ///     Registers a backup routine.
    /// </summary>
    /// <param name="type">
    ///     The routine type.
    /// </param>
    /// <returns>
    ///     The current <see cref="BackupRegistration"/> instance so other
    ///     actions can be chained.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     if <paramref name="type"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     if <paramref name="type"/> is an open generic type, is not a
    ///     concrete class or does not implement
    ///     <see cref="IBackupRoutine"/>.
    /// </exception>
    /// <remarks>
    ///     Manually registered routines are ignored when
    ///     <see cref="AssemblyScanAware{TSelf}.UseAssemblyScan"/> is
    ///     <see langword="true"/>.
    /// </remarks>
    public BackupRegistration WithRoutine(Type type) {
        Throws.When.IsOpenGenericType(type);
        Throws.When.IsNonConcreteType(type);
        Throws.When.IsNotAssignableFrom(type, typeof(IBackupRoutine));

        _routines.Add(type);

        return this;
    }
}
