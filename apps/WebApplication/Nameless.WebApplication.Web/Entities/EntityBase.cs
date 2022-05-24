namespace Nameless.WebApplication.Web.Entities {

    public abstract class EntityBase {

        #region Private Read-Only Fields

        private readonly Guid _id;
        private readonly DateTime _creationDate;

        #endregion

        #region Public Virtual Properties

        public virtual Guid Id => _id;
        public virtual DateTime CreationDate => _creationDate;
        public virtual DateTime? ModificationDate { get; set; }

        #endregion

        #region Protected Constructors

        protected EntityBase(Guid id, DateTime? creationDate = null) {
            _id = id;
            _creationDate = creationDate ?? DateTime.UtcNow;
        }

        #endregion
    }
}
