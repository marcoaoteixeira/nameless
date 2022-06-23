namespace Nameless.AspNetCore.Identity.NHibernate {

    [Serializable]
    public sealed class InvalidIDCastException : Exception {

        #region Public Constructors

        public InvalidIDCastException(string parameterName, object? id, Type castType)
            : this($"Could not cast argument {parameterName}{(id != null ? $" ({id.GetType().FullName})" : string.Empty)} as {castType}.") { }

        public InvalidIDCastException() { }
        public InvalidIDCastException(string message) : base(message) { }
        public InvalidIDCastException(string message, Exception inner) : base(message, inner) { }

        #endregion
    }
}
