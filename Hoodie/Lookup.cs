using System.Collections.Generic;
using System.Collections.Immutable;

namespace Hoodie
{
    public class Lookup<K, V>
    {
        private readonly ImmutableDictionary<K, ImmutableArray<V>> _vals;

        public Lookup()
        {
            _vals = ImmutableDictionary<K, ImmutableArray<V>>.Empty;
        }

        public IEnumerable<V> this[K key] =>
            _vals.TryGetValue(key, out var vals)
                ? vals
                : ImmutableArray<V>.Empty;
    }
}