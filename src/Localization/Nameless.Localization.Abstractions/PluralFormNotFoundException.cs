using System.Runtime.Serialization;

namespace Nameless.Localization {

    [Serializable]
    public class PluralFormNotFoundException : Exception {

        #region Public Constructors

        public PluralFormNotFoundException() { }
        public PluralFormNotFoundException(string message) : base(message) { }
        public PluralFormNotFoundException(string message, Exception inner) : base(message, inner) { }

        #endregion

        #region Protected Constructors

        protected PluralFormNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }
}
