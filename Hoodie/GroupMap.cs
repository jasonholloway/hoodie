using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Hoodie
{
    public class GroupMap<N, V> : GroupMap, IEquatable<GroupMap<N, V>>
    {
        private readonly ImmutableHashSet<Group<N, V>> _groups;
        private readonly ImmutableDictionary<N, ImmutableHashSet<Group<N, V>>> _index;

        private GroupMap(ImmutableHashSet<Group<N, V>> groups = null, ImmutableDictionary<N, ImmutableHashSet<Group<N, V>>> index = null)
        {
            _groups = groups ?? ImmutableHashSet<Group<N, V>>.Empty;
            _index = index ?? ImmutableDictionary<N, ImmutableHashSet<Group<N, V>>>.Empty;
        }

        public GroupMap<N, V> Add(Group<N, V> group)
        {
            var groups = _groups.Add(group);
            return new GroupMap<N, V>(groups, _index);
        }

        public IEnumerable<Group<N, V>> this[N node]
            => _index.TryGetValue(node, out var found) 
                ? found 
                : Enumerable.Empty<Group<N, V>>();

        public static readonly GroupMap<N, V> Empty = new GroupMap<N, V>();

        public GroupMap<N, V> Combine(GroupMap<N, V> other)
        {
            //so - to combine here then
            //
            //
            
            
            return other;
        }
        
        #region Equality
        
        public bool Equals(GroupMap<N, V> other)
            => other?._groups.SetEquals(_groups) ?? false;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GroupMap<N, V>) obj);
        }

        public override int GetHashCode()
            => _groups.GetHashCode() + 1;
        
        #endregion
    }

    public abstract class GroupMap
    {
        public static GroupMap<N, V> Combine<N, V>(GroupMap<N, V> left, GroupMap<N, V> right)
            => left.Combine(right);
    }

    
    public class Group<N, V> : Group, IEquatable<Group<N, V>>
    {
        public readonly ImmutableHashSet<N> Nodes;
        public readonly V Value;
        public readonly int _hash;

        internal Group(ImmutableHashSet<N> nodes, V value)
        {
            Nodes = nodes.ToImmutableHashSet();
            Value = value;
            _hash = nodes.Aggregate(1, (h, n) => h + n.GetHashCode() * 13) + value.GetHashCode();
        }

        public bool Equals(Group<N, V> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Nodes.SetEquals(other.Nodes) && EqualityComparer<V>.Default.Equals(Value, other.Value);
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
    }

    public abstract class Group
    {
        public static Group<N, V> From<N, V>(IEnumerable<N> nodes, V value)
            => new Group<N, V>(nodes.ToImmutableHashSet(), value);
    }
}
