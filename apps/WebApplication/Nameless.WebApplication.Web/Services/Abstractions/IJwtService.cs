using Nameless.WebApplication.Web.Entities;

namespace Nameless.WebApplication.Web.Services {

    public interface IJwtService {

        #region Methods

        string DecodeToken(string token);
        string GenerateToken(string email);
        
        #endregion
    }
}
