using System.Linq;

namespace Hoodie.GroupMaps
{
    public interface IMonoid<T>
    {
        T Zero { get; }
        T Combine(T left, T right);
    }

    public class GroupMonoid<N, V> : IMonoid<Group<N, V>>
    {
        readonly IMonoid<V> _monoidV;

        public GroupMonoid(IMonoid<V> monoidV)
        {
            _monoidV = monoidV;
        }
        
        public Group<N, V> Zero 
            => Group.From(Enumerable.Empty<N>(), _monoidV.Zero);
        
        public Group<N, V> Combine(Group<N, V> left, Group<N, V> right)
            => new Group<N, V>(
                left.Nodes.Union(right.Nodes), 
                _monoidV.Combine(left.Value, right.Value));
    }

    public class GroupMapMonoid<N, V> : IMonoid<GroupMap<N, V>>
    {
        readonly IMonoid<Group<N, V>> _monoidGroup;

        public GroupMapMonoid(IMonoid<Group<N, V>> monoidGroup)
        {
            _monoidGroup = monoidGroup;
        }
        
        public GroupMap<N, V> Zero => GroupMap<N, V>.Empty;

        public GroupMap<N, V> Combine(GroupMap<N, V> left, GroupMap<N, V> right) 
            => right.Groups.Aggregate(
                    left,
                    (ac, g) => ac.Add(g));
    }

    public class StringMonoid : IMonoid<string>
    {
        public string Zero => "";
        public string Combine(string left, string right)
            => left + right;
    }
    
}