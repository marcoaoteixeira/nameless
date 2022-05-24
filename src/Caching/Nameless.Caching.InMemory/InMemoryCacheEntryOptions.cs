using Microsoft.Extensions.Primitives;

namespace Nameless.Caching.InMemory {

    public sealed class InMemoryCacheEntryOptions : CacheEntryOptions {

		#region Public Properties

		public PostEvictionDelegate? EvictionCallback { get; set; }

		public IList<IChangeToken> ChangeTokens { get; } = new List<IChangeToken>();

		#endregion

		#region Internal Methods

		internal void OnEviction(string key, object value) {
			EvictionCallback?.Invoke(key, value);
		}

		#endregion
	}
}