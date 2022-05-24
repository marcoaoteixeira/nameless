using Nameless.WebApplication.Web.Entities;

namespace Nameless.WebApplication.Web.Services {

    public interface IUserService {

        #region Methods

        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        #endregion
    }
}
