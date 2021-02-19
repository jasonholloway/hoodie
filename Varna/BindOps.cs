using System.Collections.Immutable;
using System.Linq;

namespace Varna
{
    static class BindOps
    {
        public static ImmutableDictionary<string, Exp> InCommon(
            ImmutableDictionary<string, Exp> left,
            ImmutableDictionary<string, Exp> right)
        {
            return left.Aggregate(
                ImmutableDictionary<string, Exp>.Empty,
                (ac, kv) => 
                    (right.TryGetValue(kv.Key, out var rightVal) 
                     && kv.Value is LeafExp lv 
                     && rightVal is LeafExp rv 
                     && lv.Raw.Equals(rv.Raw))
                        ? ac.Add(kv.Key, kv.Value) 
                        : ac
            );
        }
        
    }
}