using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Hoodie.GroupMaps
{
    public abstract class GroupMap
    {
        public static Map<N, V> Lift<N, V>(IEnumerable<N> nodes, V val)
            => Map<N, V>.Lift(nodes, val);
    }

    public class Map<N, V> : IEquatable<Map<N, V>>
    {
        readonly int _gid = 0;
        readonly ImmutableSortedDictionary<int, Group<N, V>> _groups;
        readonly ImmutableDictionary<N, ImmutableHashSet<int>> _index;
        
        public IEnumerable<(int, Group<N, V>)> Groups 
            => _groups.Select(kv => (kv.Key, kv.Value));

        public IDictionary<N, ImmutableHashSet<int>> Index
            => _index;

        private Map(
            int gid = 0,    
            ImmutableSortedDictionary<int, Group<N, V>> groups = null,
            ImmutableDictionary<N, ImmutableHashSet<int>> index = null
            )
        {
            _gid = gid;
            _groups = groups ?? ImmutableSortedDictionary<int, Group<N, V>>.Empty;
            _index = index ?? ImmutableDictionary<N, ImmutableHashSet<int>>.Empty;
        }
        
        public static readonly Map<N, V> Empty = new Map<N, V>();

        public static Map<N, V> Lift(IEnumerable<N> nodes, V val)
        {
            var group = Group.From(nodes, val);
            return new Map<N, V>(
                1,
                ImmutableSortedDictionary<int, Group<N, V>>
                    .Empty.Add(0, group),
                nodes.Aggregate(
                    ImmutableDictionary<N, ImmutableHashSet<int>>.Empty,
                    (index, n) => index
                        .Add(n, ImmutableHashSet<int>.Empty.Add(0))
                    )
                );
        }

        public Map<N, V> Add(Map<N, V> other)
            => Bounce(this, other);
        
        private static Map<N, V> Bounce(Map<N, V> left, Map<N, V> right)
        {
            if (right._groups.IsEmpty) return left;
            else
            {
                var (gid, group) = right.Groups.First();
                right = right.RemoveGroup(gid);
                left = left.Add(group.Nodes, group.Value);
                return Bounce(left, right);
            }
        }
        
        private Map<N, V> Add(ImmutableHashSet<N> newNodes, V newVal)
        {
            var disjuncts = newNodes
                .SelectMany(LookupIndexed)
                .ToImmutableHashSet();
            
            var groups2 = disjuncts.Aggregate(
                _groups,
                (ac, did) =>
                {
                    var found = ac[did];
                    var found2 = found.AddDisjunct(_gid);
                    var ac2 = ac.SetItem(did, found2);
                    return ac2;
                });

            return new Map<N, V>(
                _gid + 1,
                groups2.Add(_gid, new Group<N, V>(newNodes, disjuncts, newVal)),
                newNodes.Aggregate(
                    _index,
                    (ac, n) => ac.TryGetValue(n, out var indexed) 
                        ? ac.SetItem(n, indexed.Add(_gid))
                        : ac.Add(n, ImmutableHashSet<int>.Empty.Add(_gid))
                    ));
        }

        private Map<N, V> RemoveGroup(int gid)
        {
            if (_groups.TryGetValue(gid, out var toRemove))
            {
                var groups2 = _groups.Remove(gid);
                
                var groups3 = toRemove.Disjuncts
                    .Aggregate(
                        groups2,
                        (ac, did) => ac.SetItem(did, ac[did].RemoveDisjunct(gid))
                        );

                var index = toRemove.Nodes.Aggregate(
                    _index,
                    (ac, n) => ac.SetItem(n, ac[n].Remove(gid)));
                
                return new Map<N, V>(_gid, groups3, index);
            }
            
            return this;
        }
        
        IEnumerable<int> LookupIndexed(N n)
            => _index.TryGetValue(n, out var found)
                ? found
                : Enumerable.Empty<int>();
        

        public Map<N, V> Remove(Group<N, V> group)
        {
            var found = _groups.FirstOrDefault(kv => kv.Value.Equals(group));
            return RemoveGroup(found.Key);
        }

        public IEnumerable<Group<N, V>> this[N node]
            => _index.TryGetValue(node, out var gids) 
                ? gids.Select(gid => _groups[gid])
                : Enumerable.Empty<Group<N, V>>();

        public override string ToString()
            => $"<{string.Join(",", _groups.Select(g => g.Value))}>";

        #region Equality
        
        public bool Equals(Map<N, V> other)
            => other?._groups.Values.ToImmutableHashSet().SetEquals(_groups.Values) ?? false;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Map<N, V>) obj);
        }

        public override int GetHashCode()
            => _groups.GetHashCode() + 1;
        
        #endregion
    }
}
