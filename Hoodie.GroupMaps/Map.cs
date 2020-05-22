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

        public IEnumerable<Group<N, V>> Groups
            => _groups.Values;

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
                    .Empty
                    .Add(0, group),
                nodes.Aggregate(
                    ImmutableDictionary<N, ImmutableHashSet<int>>
                        .Empty,
                    (index, n) => index
                        .Add(n, ImmutableHashSet<int>.Empty.Add(0))
                    )
                );
        }

        public Map<N, V> Combine(Map<N, V> other, IMonoid<V> mV) 
            => Bounce(this, other, mV);
        
        //as we start bouncing, disjuncts should be separately projected
        //our creations, by being added separately, will be as disjuncts as the sources therefore
        
        //this requires the right hand groups themselves to be clumped
        //
        //

        delegate T Op<T>(T inp);

        private static Map<N, V> Bounce(Map<N, V> left, Map<N, V> right, IMonoid<V> mV)
        {
            if (right._groups.IsEmpty) return left;
            else
            {
                var froms = Enumerable.Concat(
                        right.Groups.Take(1),
                        right.Groups.First()
                            .Disjuncts.Select(gid => right._groups[gid])
                    )
                    .ToArray();

                right = froms.Aggregate(right,
                    (m, g) => m.RemoveGroup(g.Gid));

                (left, right) = froms.Aggregate(
                    (left, right),
                    (tup, toAdd) =>
                    {
                        var (_left, _right) = tup;
                        
                        var _hits = toAdd.Nodes
                            .SelectMany(n => left[n])
                            .ToImmutableHashSet();
                        
                        if (_hits.IsEmpty)
                        {
                            return (
                                _left.Add(toAdd.Nodes, toAdd.Value), 
                                _right
                                );
                        }
                        else
                        {
                            //but some of these hits will be partial - ie undermined by other disjuncts out of view
                            //
                            
                            return ClumpHits(_hits)
                                .Aggregate(
                                    (_left, _right),
                                    (_tup, clump) =>
                                    {
                                        var (__left, __right) = _tup;
                                        return (
                                            clump
                                                .Aggregate(__left, (ll, g) => ll.RemoveGroup(g.Gid))
                                                .Add(
                                                    toAdd.Nodes
                                                        .Concat(clump.SelectMany(g => g.Nodes))
                                                        .ToImmutableHashSet(),
                                                    clump.Aggregate(toAdd.Value, (v, g) => mV.Combine(v, g.Value))
                                                ),
                                            __right
                                            );
                                    }); 
                        }
                    });
                
                return Bounce(left, right, mV);
                
                //so, we want an empty group to be emitted if any of the hit groups is fractured
                //and what is a fractured group? one that is in disjunction
                
                //if there are no hits, then we should be returning an empty group surely
                //yes - no hits: empty group returned please
                //similarly, fractured group: empty group as disjunct
                
                IEnumerable<ImmutableHashSet<Group<N, V>>> ClumpHits(ImmutableHashSet<Group<N, V>> _hits)
                {
                    switch (_hits.Count)
                    {
                        case 0: return Enumerable.Empty<ImmutableHashSet<Group<N, V>>>();
                        case 1: return new[] { _hits };
                        default:
                        {
                            var head = _hits.First();
                            var tail = _hits.Remove(head);

                            var clumps = ClumpHits(tail
                                    .Where(g => !g.Disjuncts.Contains(head.Gid))
                                    .ToImmutableHashSet())
                                .Select(set => set.Add(head))
                                .ToImmutableHashSet();

                            if (clumps.IsEmpty)
                            {
                                clumps = new[] { new[] { head }.ToImmutableHashSet() }.ToImmutableHashSet();
                            }

                            var tail2 = clumps
                                .SelectMany(set => set)
                                .Aggregate(tail, (ac, g) => ac.Remove(g));

                            return clumps
                                .Concat(ClumpHits(tail2));
                        }
                    }
                }
            }
        }

        public Map<N, V> Add(Map<N, V> other)
            => other.Groups
                .Aggregate(this,
                    (ac, g) => ac.Add(g.Nodes, g.Value));
        
        private Map<N, V> Add(IEnumerable<N> newNodes, V newVal)
        {
            var _newNodes = newNodes.ToImmutableHashSet();
            
            var disjuncts = _newNodes
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
                groups2.Add(_gid, new Group<N, V>(_gid, _newNodes, disjuncts, newVal)),
                _newNodes.Aggregate(
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
