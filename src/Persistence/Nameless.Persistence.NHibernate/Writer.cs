using NHibernate;
using NHibernate.Linq;

namespace Nameless.Persistence.NHibernate {

    public sealed class Writer : IWriter {

        #region Private Read-Only Fields

        private readonly ISession _session;

        #endregion

        #region Public Constructors

        public Writer(ISession session) {
            Prevent.Null(session, nameof(session));

            _session = session;
        }

        #endregion

        #region IWriter Members

        public Task SaveAsync<TEntity>(SaveInstructionCollection<TEntity> instructions, CancellationToken cancellationToken = default) where TEntity : class {
            if (instructions.IsNullOrEmpty()) { return Task.CompletedTask; }

            using var transaction = _session.BeginTransaction();
            try {
                foreach (var instruction in instructions) {
                    var query = instruction.Filter != null
                        ? _session.Query<TEntity>().Where(instruction.Filter)
                        : null;

                    switch (instruction.Type) {
                        case WriteType.Insert:
                            _session.Save(instruction.Entity);
                            break;
                        case WriteType.Update:
                            if (query != null) { query.Update(_ => instruction.Entity); }
                            else { _session.Update(instruction.Entity); }
                            break;
                        case WriteType.Upsert:
                            if (query != null && query.Any()) { query.Update(_ => instruction.Entity); }
                            else { _session.SaveOrUpdate(instruction.Entity); }
                            break;
                    }
                }
                transaction.Commit();
            } catch { transaction.Rollback(); throw; }

            return Task.CompletedTask;
        }

        public Task DeleteAsync<TEntity>(DeleteInstructionCollection<TEntity> instructions, CancellationToken cancellationToken = default) where TEntity : class {
            if (instructions.IsNullOrEmpty()) { return Task.CompletedTask; }

            using var transaction = _session.BeginTransaction();
            try {
                foreach (var instruction in instructions) {
                    _session.Query<TEntity>().Where(instruction.Filter).Delete();
                }
                transaction.Commit();
            } catch { transaction.Rollback(); throw; }

            return Task.CompletedTask;
        }

        #endregion
    }
}
