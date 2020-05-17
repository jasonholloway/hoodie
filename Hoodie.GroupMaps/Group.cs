using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Hoodie.GroupMaps
{
    public abstract class Group
    {
        public static Group<N, V> From<N, V>(IEnumerable<N> nodes, V value)
            => new Group<N, V>(
                -1,
                nodes.ToImmutableHashSet(), 
                ImmutableHashSet<int>.Empty, 
                value);
    }

    public class Group<N, V> : Group
    {
        public readonly int Gid;
        
        public readonly ImmutableHashSet<N> Nodes;
        public readonly ImmutableHashSet<int> Disjuncts;
        public readonly V Value;
        public readonly int _hash;

        internal Group(int gid, ImmutableHashSet<N> nodes, ImmutableHashSet<int> disjuncts, V value)
        {
            Gid = gid;
            Nodes = nodes;
            Disjuncts = disjuncts;
            Value = value;
            _hash = nodes.Aggregate(1, (h, n) => h + n.GetHashCode() * 13) + value.GetHashCode() +
                    disjuncts.GetHashCode();
        }

        internal Group<N, V> AddDisjunct(int gid)
            => new Group<N, V>(Gid, Nodes, Disjuncts.Add(gid), Value);

        internal Group<N, V> RemoveDisjunct(int gid)
            => new Group<N, V>(Gid, Nodes, Disjuncts.Remove(gid), Value);

        public bool Equals(Group<N, V> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Nodes.SetEquals(other.Nodes)
                   && EqualityComparer<V>.Default.Equals(Value, other.Value)
                   && Disjuncts.SetEquals(other.Disjuncts);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Group<N, V>) obj);
        }

        public override int GetHashCode()
            => _hash;

        public override string ToString()
            => $"([{string.Join(",", Nodes)}], {Value})";
    }
}