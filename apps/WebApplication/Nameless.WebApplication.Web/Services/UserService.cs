using Nameless.Persistence;
using Nameless.WebApplication.Web.Entities;

namespace Nameless.WebApplication.Web.Services {

    public sealed class UserService : IUserService {

        #region Private Read-Only Fields

        private readonly IRepository _repository;

        #endregion

        #region Public Constructors

        public UserService(IRepository repository) {
            Prevent.Null(repository, nameof(repository));

            _repository = repository;
        }

        #endregion

        #region IUserService Members
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) {
            var user = _repository.Query<User>().SingleOrDefault(_ => _.Email == email);
            return Task.FromResult(user);
        } 
        #endregion
    }
}
