using Nameless.WebApplication.Web.Entities;
using Nameless.WebApplication.Web.Persistence;

namespace Nameless.WebApplication.Web.Services {

    public sealed class UserService : IUserService {

        #region Private Read-Only Fields

        private readonly IRepository _repository;

        #endregion

        #region Public Constructors

        public UserService(IRepository repository) {
            Ensure.NotNull(repository, nameof(repository));

            _repository = repository;
        }

        #endregion

        #region IUserService Members
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) {
            var user = _repository
                .Query<User>()
                .SingleOrDefault(_ => string.Equals(_.Email, email, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(user);
        } 
        #endregion
    }
}
