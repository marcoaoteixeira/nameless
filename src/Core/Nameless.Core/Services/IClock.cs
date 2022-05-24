namespace Nameless.Services {

    public interface IClock {

        #region Properties

        DateTime UtcNow { get; }
        DateTimeOffset OffsetUtcNow { get; }

        #endregion
    }
}
