using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Hoodie.GroupMaps
{
    public abstract class GroupMap
    {
        public static GroupMap<N, V> From<N, V>(IEnumerable<N> nodes, V val)
            => new GroupMap<N, V>(
                new[] { Group.From(nodes, val) }.ToImmutableHashSet(),
                ImmutableDictionary<N, ImmutableHashSet<Group<N, V>>>.Empty);
    }

    public class GroupMap<N, V> : IEquatable<GroupMap<N, V>>
    {
        internal readonly ImmutableHashSet<Group<N, V>> Groups;
        internal readonly ImmutableDictionary<N, ImmutableHashSet<Group<N, V>>> Index;

        internal GroupMap(ImmutableHashSet<Group<N, V>> groups = null, ImmutableDictionary<N, ImmutableHashSet<Group<N, V>>> index = null)
        {
            Groups = groups ?? ImmutableHashSet<Group<N, V>>.Empty;
            Index = index ?? ImmutableDictionary<N, ImmutableHashSet<Group<N, V>>>.Empty;
        }
        
        public static readonly GroupMap<N, V> Empty = new GroupMap<N, V>();

        public GroupMap<N, V> Add(Group<N, V> group)
        {
            var groups = Groups.Add(group);

            var index = group.Nodes.Aggregate(
                Index,
                (ac, n) => ac.SetItem(n, 
                    ac.TryGetValue(n, out var choice) 
                        ? choice.Add(@group) 
                        : ImmutableHashSet<Group<N, V>>.Empty.Add(@group))
                );
            
            return new GroupMap<N, V>(groups, index);
        }

        public GroupMap<N, V> Remove(Group<N, V> group)
        {
            var groups = Groups.Remove(group);

            var index = group.Nodes.Aggregate(
                Index,
                (ac, n) => ac.TryGetValue(n, out var choice) 
                    ? ac.SetItem(n, choice.Remove(group)) 
                    : ac
                );
            
            return new GroupMap<N, V>(groups, index);
        }

        public IEnumerable<Group<N, V>> this[N node]
            => Index.TryGetValue(node, out var found) 
                ? found 
                : Enumerable.Empty<Group<N, V>>();

        public override string ToString()
            => $"<{string.Join(",", Groups)}>";

        #region Equality
        
        public bool Equals(GroupMap<N, V> other)
            => other?.Groups.SetEquals(Groups) ?? false;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GroupMap<N, V>) obj);
        }

        public override int GetHashCode()
            => Groups.GetHashCode() + 1;
        
        #endregion
    }
}
