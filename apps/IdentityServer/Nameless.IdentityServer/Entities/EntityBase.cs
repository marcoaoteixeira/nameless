namespace Nameless.IdentityServer.Entities {

    public abstract class EntityBase {

        #region Public Virtual Properties

        public virtual DateTime CreationDate { get; set; }
        public virtual DateTime ModificationDate { get; set; }

        #endregion
    }
}
