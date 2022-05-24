namespace Nameless.WebApplication.Web.Services {
    
    public interface ICacheService {

        #region Methods

        Task StoreAsync(string key, object value, DateTimeOffset expiration, CancellationToken cancellationToken = default);
        Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);

        #endregion
    }
}
