using System.Linq.Expressions;
using Nameless.Helpers;
using Nameless.WebApplication.Web.Entities;

namespace Nameless.WebApplication.Web.Persistence {

    public static class RepositoryExtension {

        #region Public Static Methods

        public static TEntity? Save<TEntity>(this IRepository self, TEntity entity) where TEntity : EntityBase {
            Ensure.NotNull(self, nameof(self));

            return AsyncHelper.RunSync(() => self.SaveAsync(entity));
        }

        public static bool Delete<TEntity>(this IRepository self, TEntity entity) where TEntity : EntityBase {
            Ensure.NotNull(self, nameof(self));

            return AsyncHelper.RunSync(() => self.DeleteAsync(entity));
        }

        public static bool Exists<TEntity>(this IRepository self, Expression<Func<TEntity, bool>> filter) where TEntity : EntityBase {
            Ensure.NotNull(self, nameof(self));

            return AsyncHelper.RunSync(() => self.ExistsAsync(filter));
        }

        #endregion
    }
}
