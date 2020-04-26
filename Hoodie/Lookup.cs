using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Hoodie
{
    public class Lookup<K, V>
    {
        private readonly ImmutableDictionary<K, ImmutableArray<V>> _rels;

        public Lookup(params (IEnumerable<K>, IEnumerable<V>)[] rels)
        {
            _rels = rels.Aggregate(
                ImmutableDictionary<K, ImmutableArray<V>>.Empty,
                (acc1, rel) =>
                {
                    var (keys, vals) = rel;
                    return keys.Aggregate(
                        acc1, 
                        (acc2, key) => 
                            acc2.TryGetValue(key, out var found) 
                                ? acc2.SetItem(key, found.AddRange(vals)) 
                                : acc2.Add(key, ImmutableArray<V>.Empty.AddRange(vals)
                                )
                        );
                });
        }

        public IEnumerable<V> this[K key] =>
            _rels.TryGetValue(key, out var vals)
                ? vals
                : ImmutableArray<V>.Empty;
    }
}