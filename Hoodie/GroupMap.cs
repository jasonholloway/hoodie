using System;
using System.Collections.Generic;

namespace Hoodie
{
    public class GroupMap<N, V> : GroupMap
    {
        private GroupMap()
        { }

        public GroupMap<N, V> Add(Group<N, V> group)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Group<N, V>> this[N node]
            => throw new NotImplementedException();

        public static readonly GroupMap<N, V> Empty = new GroupMap<N, V>();
    }

    public abstract class GroupMap
    {
        public static GroupMap<N, V> Combine<N, V>(GroupMap<N, V> left, GroupMap<N, V> right)
            => throw new NotImplementedException();
    }

    public class Group<N, V> : Group
    {
        public readonly IEnumerable<N> Nodes;
        public readonly V Value;

        public Group(IEnumerable<N> nodes, V value)
        {
            Nodes = nodes;
            Value = value;
        }
    }

    public abstract class Group
    {
        public static Group<N, V> From<N, V>(IEnumerable<N> nodes, V value)
            => new Group<N, V>(nodes, value);
    }
}
