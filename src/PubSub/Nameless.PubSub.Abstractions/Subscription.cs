using System.Reflection;

namespace Nameless.PubSub {

    /// <summary>
    /// Represents the subscription for the message handler.
    /// </summary>
    public sealed class Subscription : IDisposable {

        #region Private Fields

        private MethodInfo? _methodInfo;
        private WeakReference? _targetObject;
        private bool _isStatic;
        private bool _disposed;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the topic of the subscription
        /// </summary>
        public string Topic { get; }

        /// <summary>
        /// Gets the method that will handle the message.
        /// </summary>
        public MemberInfo HandlerInfo {
            get { return _methodInfo!; }
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="Subscription"/>.
        /// </summary>
        /// <param name="topic">The subscription topic.</param>
        /// <param name="handler">The message handler.</param>
        public Subscription(string topic, Action<Message> handler) {
            Ensure.NotNullEmptyOrWhiteSpace(topic, nameof(topic));
            Ensure.NotNull(handler, nameof(handler));

            Topic = topic;

            _methodInfo = handler.GetMethodInfo();
            _targetObject = new WeakReference(handler.Target);
            _isStatic = handler.Target == null;
        }

        #endregion

        #region Destructor

        /// <summary>
        /// Destructor
        /// </summary>
        ~Subscription() => Dispose(disposing: false);

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a handler for the subscription.
        /// </summary>
        /// <returns>An instance of <see cref="Action{Message}" />.</returns>
        public Action<Message>? CreateHandler() {
            BlockAccessAfterDispose();

            if (_targetObject!.Target != null && _targetObject.IsAlive) {
                return (Action<Message>)_methodInfo!.CreateDelegate(typeof(Action<Message>), _targetObject.Target);
            }

            if (_isStatic) {
                return (Action<Message>)_methodInfo!.CreateDelegate(typeof(Action<Message>));
            }

            return null;
        }

        /// <summary>
        /// Checks for equality of the object.
        /// </summary>
        /// <param name="obj">The other <see cref="Subscription" /> object.</param>
        /// <returns><c>true</c> if equals, otherwise <c>false</c>.</returns>
        public bool Equals(Subscription? obj) => obj != null
            && obj.Topic == Topic
            && obj.HandlerInfo == HandlerInfo;

        #endregion

        #region Public Override Methods

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as Subscription);

        /// <inheritdoc />
        public override int GetHashCode() {
            var hash = 13;
            unchecked {
                hash += Topic.GetHashCode() * 7;
                hash += HandlerInfo.GetHashCode() * 7;
            }
            return hash;
        }

        #endregion

        #region Private Methods

        private void BlockAccessAfterDispose() {
            if (_disposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private void Dispose(bool disposing) {
            if (_disposed) { return; }
            if (disposing) { /* Dispose managed resources */ }
            // Dispose unmanaged resources

            _methodInfo = null;
            _targetObject = null;
            _isStatic = false;

            _disposed = true;
        }

        #endregion

        #region IDisposable Members

        /// <inheritdoc />
        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}