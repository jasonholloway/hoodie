using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Hoodie.GroupMaps
{
    public abstract class Disjunction 
    {
        public static Disjunction<N, V> From<N, V>(IEnumerable<Map<N, V>> disjuncts)
            => new Disjunction<N, V>(disjuncts);
    }
    
    public class Disjunction<N, V> : Disjunction, IEquatable<Disjunction<N, V>>
    {
        public readonly ImmutableHashSet<Map<N, V>> Disjuncts;
        readonly int _hash;

        public Disjunction(IEnumerable<Map<N, V>> disjuncts)
        {
            Disjuncts = disjuncts.ToImmutableHashSet();
            _hash = Disjuncts.Aggregate(13, (ac, m) => ac + 13 * m.GetHashCode());
        }

        public override string ToString()
            => $"| {string.Join(" ^ ", Disjuncts)} |";

        public bool Equals(Disjunction<N, V> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return GetHashCode() == other.GetHashCode() 
                   && Disjuncts.SetEquals(other.Disjuncts);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Disjunction<N, V>) obj);
        }

        public override int GetHashCode() 
            => _hash;
    }
}