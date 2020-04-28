using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Hoodie
{
    public class Lookup<K, V>
    {
        private readonly ImmutableDictionary<K, ImmutableArray<V>> _rels;

        private Lookup(ImmutableDictionary<K, ImmutableArray<V>> rels)
        {
            _rels = rels;
        }

        public Lookup(params (IEnumerable<K>, IEnumerable<V>)[] rels)
            : this(rels.Aggregate(
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
                }))
        { }

        public IEnumerable<V> this[K key] =>
            _rels.TryGetValue(key, out var vals)
                ? vals
                : ImmutableArray<V>.Empty;

        public static Lookup<K, V> Combine(Lookup<K, V> left, Lookup<K, V> right)
        {
            var lRels = left._rels;
            var rRels = right._rels;
            
            //so this is just concatenating the dijuncts: they must be multiplied
            //think of each valuein the lok up really being a disjunction
            
            //but this first layer of disjunction on the portset level
            //howis this here combined together?
            //
            //

            var combined = rRels.Aggregate(lRels, (ac, kv) =>
            {
                var r = ac.TryGetValue(kv.Key, out var found) 
                    ? found 
                    : ImmutableArray<V>.Empty;

                return ac.SetItem(kv.Key, r.AddRange(kv.Value));
            });

            return new Lookup<K, V>(combined);
        }
    }
}