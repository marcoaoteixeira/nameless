using Newtonsoft.Json;

namespace Nameless.Utils {

    public static class DeepCopy {

        #region Public Static Methods

        /// <summary>
        /// Performs a deep copy of an object, using JSON as a serialization method. NOTE: Private members are not cloned using this method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="obj">The object to copy.</param>
        /// <returns>The copied object.</returns>
        /// <remarks>
        /// Reference article http://www.codeproject.com/KB/tips/SerializedObjectCloner.aspx
        /// </remarks>
        public static object? Clone(object obj) {
            // Don't serialize a null object, simply return the default for that object.
            if (obj is null) { return default; }

            var objType = obj.GetType();
            if (objType.IsAbstract || objType.IsInterface || objType.IsPointer) {
                throw new InvalidOperationException($"Cannot clone abstract classes, interfaces or pointers. Object type: {objType}");
            }

            // Initialize inner objects individually, for example in default constructor
            // some list property initialized with some values, but in 'instance' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var settings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
            var json = JsonConvert.SerializeObject(obj);

            return JsonConvert.DeserializeObject(json, objType, settings);
        }

        public static T? Clone<T>(T obj) where T : class => Clone(obj as object) as T;

        #endregion
    }
}