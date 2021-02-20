using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Varna
{
    using static Ops;
    
    public class Binds : IEquatable<Binds>
    {
        readonly ImmutableDictionary<string, Exp> _dict;
        readonly int _hash;

        public static readonly Binds Empty = new Binds(ImmutableDictionary<string, Exp>.Empty, 412492);

        private Binds(ImmutableDictionary<string, Exp> dict, int hash)
        {
            _dict = dict;
            _hash = hash;
        }

        public Exp Get(string name)
            => _dict.TryGetValue(name, out var found)
                ? found
                : new True();

        public Binds Set(string name, Exp exp)
        {
            var h = _hash;
            
            if (_dict.TryGetValue(name, out var found))
            {
                h -= HashCode.Combine(name, ScopeComparer.Exp.GetHashCode(found));
            }

            h += HashCode.Combine(name, ScopeComparer.Exp.GetHashCode(exp));

            return new Binds(_dict.SetItem(name, exp), h);
        }
        
        public Binds Combine(Binds other)
        {
            var keys = _dict.Keys
                .Concat(other._dict.Keys)
                .Distinct();

            return keys.Aggregate(
                Empty,
                (ac, k) =>
                {
                    if (ac == null) return null;
                    
                    var lv = Get(k);
                    var rv = other.Get(k);
                    
                    return (lv, rv) switch
                    {
                        (True, _) => ac.Set(k, rv),
                        (_, True) => ac.Set(k, lv),
                        (Int l, Int r) when l.Value == r.Value => ac.Set(k, l),
                        (LeafExp l, LeafExp r) when l.Raw.Equals(r.Raw) => ac.Set(k, l),
                        _ => null
                    };
                });
        }

        public Binds Intersect(Binds other)
        {
            return _dict.Aggregate(
                Empty,
                (ac, kv) => 
                    (other._dict.TryGetValue(kv.Key, out var rightVal) 
                     && kv.Value is LeafExp lv 
                     && rightVal is LeafExp rv 
                     && lv.Raw.Equals(rv.Raw))
                        ? ac.Set(kv.Key, kv.Value) 
                        : ac
            );
        }

        public bool Equals(Binds other)
            => _hash == other?._hash
               && _dict.OrderBy(kv => kv.Key)
                   .SequenceEqual(other._dict.OrderBy(kv => kv.Key), KVComparer.Instance);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Binds) obj);
        }

        public override int GetHashCode()
            => _hash;
    }

    class KVComparer : IEqualityComparer<KeyValuePair<string, Exp>>
    {
        public static readonly IEqualityComparer<KeyValuePair<string, Exp>> Instance = new KVComparer();
            
        public bool Equals(KeyValuePair<string, Exp> x, KeyValuePair<string, Exp> y)
            => x.Key == y.Key 
               && ScopeComparer.Exp.Equals(x.Value, y.Value);

        public int GetHashCode(KeyValuePair<string, Exp> obj)
            => HashCode.Combine(
                obj.Key, 
                ScopeComparer.Exp.GetHashCode(obj.Value));
    }
}