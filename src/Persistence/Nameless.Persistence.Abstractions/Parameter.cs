using System.Collections;

namespace Nameless.Persistence {

    /// <summary>
    /// Represents a directive parameter.
    /// </summary>
    public sealed class Parameter {

        #region Public Properties

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the value of the parameter.
        /// </summary>
        public object Value { get; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="Parameter" />.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        public Parameter(string name, object value) {
            Ensure.NotNullEmptyOrWhiteSpace(name, nameof(name));

            Name = name;
            Value = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves the typed parameter value. If defined value is not of the type <typeparamref name="T" />
        /// tries to change type.
        /// </summary>
        /// <typeparam name="T">Type to convert.</typeparam>
        /// <returns>The typed parameter value.</returns>
        public T GetValue<T>() {
            if (Value is T value) { return value; }

            return (T)Convert.ChangeType(Value, typeof(T));
        }

        public bool TryGetValue<T>(out T? output) {
            output = default;
            try {
                output = Value is not T value
                    ? (T)Convert.ChangeType(Value, typeof(T))
                    : value;

                return true;
            } catch { return false; }
        }

        /// <summary>
        /// Checks if the current instance is equals to the specified instance.
        /// </summary>
        /// <param name="obj">The <see cref="Parameter"/> instance to check.</param>
        /// <returns><c>true</c> if equals; otherwise <c>false</c></returns>
        public bool Equals(Parameter? obj) {
            return obj != null && string.Equals(obj.Name, Name, StringComparison.CurrentCultureIgnoreCase);
        }

        #endregion

        #region Public Override Methods

        /// <inheritdoc />
        public override bool Equals(object? obj) {
            return Equals(obj as Parameter);
        }

        /// <inheritdoc />
        public override int GetHashCode() {
            var hash = 13;
            unchecked {
                hash += (Name ?? string.Empty).GetHashCode() * 7;
            }
            return hash;
        }

        #endregion
    }

    /// <summary>
    /// Represents a set of directive parameters.
    /// </summary>
    public sealed class ParameterSet : IEnumerable<Parameter> {

        #region Private Read-Only Fields

        private readonly IDictionary<string, Parameter> _items;

        #endregion

        #region Public Properties

        public Parameter? this[string name] {
            get => GetParameter(name);
            set => SetParameter(name, value);
        }

        #endregion

        #region Public Constructors

        public ParameterSet(IEnumerable<Parameter>? collection = null) {
            _items = (collection ?? Enumerable.Empty<Parameter>()).ToDictionary(_ => _.Name, _ => _);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new parameter to the set.
        /// </summary>
        /// <param name="name">Name of the parameter.</param>
        /// <param name="value">Value of the parameter.</param>
        /// <returns><c>true</c> if added; otherwise <c>false</c></returns>
        public bool Add(string name, object value) {
            return Add(new Parameter(name, value));

        }
        /// <summary>
        /// Adds a new parameter to the set.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns><c>true</c> if added; otherwise <c>false</c></returns>
        public bool Add(Parameter parameter) {
            Ensure.NotNull(parameter, nameof(parameter));

            if (!_items.ContainsKey(parameter.Name)) {
                _items[parameter.Name] = parameter;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Removes a parameter from the set.
        /// </summary>
        /// <param name="name">Name of the parameter.</param>
        /// <returns><c>true</c> if removed; otherwise <c>false</c></returns>
        public bool Remove(string name) {
            Ensure.NotNullEmptyOrWhiteSpace(name, nameof(name));

            return _items.Remove(name);
        }
        /// <summary>
        /// Removes a parameter from the set.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns><c>true</c> if removed; otherwise <c>false</c></returns>
        public bool Remove(Parameter parameter) {
            Ensure.NotNull(parameter, nameof(parameter));

            return _items.Remove(parameter.Name);
        }

        #endregion

        #region Private Methods

        private Parameter? GetParameter(string name) {
            Ensure.NotNullEmptyOrWhiteSpace(name, nameof(name));

            return _items.ContainsKey(name) ? _items[name] : null;
        }

        private void SetParameter(string name, Parameter? parameter) {
            Ensure.NotNullEmptyOrWhiteSpace(name, nameof(name));
            Ensure.NotNull(parameter, nameof(parameter));

            _items[name] = parameter!;
        }

        #endregion

        #region IEnumerable Members

        /// <inheritdoc />
        public IEnumerator<Parameter> GetEnumerator() {
            return _items.Values.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)_items.Values).GetEnumerator();
        }

        #endregion
    }
}