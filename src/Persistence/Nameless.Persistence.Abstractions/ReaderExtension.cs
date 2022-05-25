using System.Linq.Expressions;
using Nameless.Helpers;

namespace Nameless.Persistence {

    public static class ReaderExtension {

        #region Public Static Methods

        public static IList<TEntity> Find<TEntity>(this IReader self, Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, object>>? orderBy = null, bool orderDescending = false) where TEntity : class {
            Prevent.Null(self, nameof(self));

            return AsyncHelper.RunSync(() => self.FindAsync(filter, orderBy, orderDescending));
        }

        public static bool Exists<TEntity>(this IReader self, Expression<Func<TEntity, bool>> filter) where TEntity : class {
            Prevent.Null(self, nameof(self));

            return AsyncHelper.RunSync(() => self.ExistsAsync(filter));
        }

        #endregion
    }
}
