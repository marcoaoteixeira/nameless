namespace Nameless.Persistence {

    /// <summary>
    /// Exposes methods to define a data writer.
    /// </summary>
    public interface IWriter {

        #region Methods

        Task SaveAsync<TEntity>(SaveInstructionCollection<TEntity> instructions, CancellationToken cancellationToken = default) where TEntity : class;

        Task DeleteAsync<TEntity>(DeleteInstructionCollection<TEntity> instructions, CancellationToken cancellationToken = default) where TEntity : class;

        #endregion
    }
}