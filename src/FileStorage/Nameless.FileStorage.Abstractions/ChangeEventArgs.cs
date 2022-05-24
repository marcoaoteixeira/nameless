namespace Nameless.FileStorage {

    public sealed class ChangeEventArgs : EventArgs {

        #region Public Properties

        public string? OriginalPath { get; set; }
        public string? CurrentPath { get; set; }
        public ChangeReason Reason { get; set; }

        #endregion

        #region Public Static Properties

        public new static ChangeEventArgs Empty => new();

        #endregion
    }

    public enum ChangeReason : int {

        None,

        Changed,

        Created,

        Deleted,

        Renamed
    }
}