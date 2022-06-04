using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;
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

        #region Private Static Methods

        private static int Insert<TEntity>(ISession session, TEntity entity)
            where TEntity : class {
            return session.Save(entity) != null ? 1 : 0;
        }

        private static int Update<TEntity>(ISession session, TEntity entity, Expression<Func<TEntity, bool>>? filter = null, Expression<Func<TEntity, TEntity>>? patch = null)
            where TEntity : class {
            if (filter != null && patch != null) {
                return session.Query<TEntity>().Where(filter).Update(patch);
            }

            if (entity != null && filter != null) {
                var count = session.Query<TEntity>().Count(filter);
                if (count == 0) { return 0; }
                if (count > 1) { throw new MultipleEntitiesFoundException(); }
                return session.Merge(entity) != null ? 1 : 0;
            }

            var exists = entity != null && Exists(session, entity);
            return exists && session.Merge(entity) != null ? 1 : 0;
        }

        private static int UpSert<TEntity>(ISession session, TEntity entity, Expression<Func<TEntity, bool>>? filter = null)
            where TEntity : class {
            Prevent.Null(entity, nameof(entity));

            var exists = false;
            if (entity != null && filter != null) {
                var count = session.Query<TEntity>().Count(filter);
                if (count > 1) { throw new MultipleEntitiesFoundException(); }
                exists = count == 1;
            }

            if (entity != null && filter == null) {
                exists = Exists(session, entity);
            }

            return exists
                    ? session.Merge(entity) != null ? 1 : 0
                    : session.Save(entity) != null ? 1 : 0;
        }

        private static bool Exists<TEntity>(ISession session, TEntity entity) where TEntity : class {
            var currentID = IDAttribute.GetID(entity);
            if (currentID == ID.Null) {
                throw new IDNotFoundException(typeof(TEntity));
            }

            var value = session
                .CreateCriteria<TEntity>()
                .Add(Restrictions.Eq(currentID.Name, currentID.Value))
                .SetProjection(Projections.RowCount())
                .UniqueResult<int>();

            return value > 0;
        }

        #endregion

        #region IWriter Members

        /// <inheritdocs />
        public Task<int> SaveAsync<TEntity>(SaveInstructionCollection<TEntity> instructions, CancellationToken cancellationToken = default) where TEntity : class {
            if (instructions.IsNullOrEmpty()) { return Task.FromResult(0); }

            var counter = 0;
            using var transaction = _session.BeginTransaction();
            try {
                foreach (var instruction in instructions) {
                    cancellationToken.ThrowIfCancellationRequested();

                    switch (instruction.Mode) {
                        case SaveMode.Insert:
                            counter += Insert(_session, instruction.Entity);
                            break;
                        case SaveMode.Update:
                            counter += Update(_session, instruction.Entity, instruction.Filter, instruction.Patch);
                            break;
                        case SaveMode.UpSert:
                            counter += UpSert(_session, instruction.Entity, instruction.Filter);
                            break;
                    }
                }
                transaction.Commit();
                _session.Flush();
            } catch { transaction.Rollback(); throw; }

            return Task.FromResult(counter);
        }

        /// <inheritdocs />
        public Task<int> DeleteAsync<TEntity>(DeleteInstructionCollection<TEntity> instructions, CancellationToken cancellationToken = default) where TEntity : class {
            if (instructions.IsNullOrEmpty()) { return Task.FromResult(0); }

            var counter = 0;
            using var transaction = _session.BeginTransaction();
            try {
                foreach (var instruction in instructions) {
                    cancellationToken.ThrowIfCancellationRequested();

                    counter += _session
                        .Query<TEntity>()
                        .Where(instruction.Filter)
                        .Delete();
                }
                transaction.Commit();
            } catch { transaction.Rollback(); throw; }

            return Task.FromResult(counter);
        }

        #endregion
    }
}
