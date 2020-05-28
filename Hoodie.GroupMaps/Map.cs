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
        readonly ImmutableSortedDictionary<N, ImmutableHashSet<int>> _index;
        readonly int _hash;

        public IEnumerable<Group<N, V>> Groups
            => _groups.Values;

        public IDictionary<N, ImmutableHashSet<int>> Index
            => _index;

        private Map(
            int gid = 0,    
            ImmutableSortedDictionary<int, Group<N, V>> groups = null,
            ImmutableSortedDictionary<N, ImmutableHashSet<int>> index = null
            )
        {
            _gid = gid;
            _groups = groups ?? ImmutableSortedDictionary<int, Group<N, V>>.Empty;
            _index = index ?? ImmutableSortedDictionary<N, ImmutableHashSet<int>>.Empty;
            _hash = _groups.Aggregate(17, (ac, g) => (ac + g.GetHashCode() * 3) + 5);
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
                    ImmutableSortedDictionary<N, ImmutableHashSet<int>>
                        .Empty,
                    (index, n) => index
                        .Add(n, ImmutableHashSet<int>.Empty.Add(0))
                    )
                );
        }

        public Map<N, V> Combine(Map<N, V> other, IMonoid<V> mV) 
            => Bounce(this, other, mV);
        


        public Disjunction<N, V> Hit(ISet<N> nodes)
        {
            var todo = nodes
                .SelectMany(n => this[n])
                .ToImmutableDictionary(g => g.Gid);

            var clumps = todo.Values
                .SelectMany(g => Clump(g, ImmutableHashSet<int>.Empty, todo))
                .ToArray();

            return Disjunction.From(
                clumps.SelectMany(s => s
                    .Select(gid => _groups[gid])
                    .Select(g => GroupMap.Lift(g.Nodes, g.Value))
                ));
            

            IEnumerable<ImmutableHashSet<int>> Clump(Group<N, V> curr, ImmutableHashSet<int> bad1, ImmutableDictionary<int, Group<N, V>> todo1)
            {
                var bad2 = curr.Disjuncts.Aggregate(bad1, (ac, did) => ac.Add(did));
                var todo2 = todo1.Remove(curr.Gid);
                var todo3 = todo2.RemoveRange(bad2); //urgh

                if (todo3.IsEmpty)
                {
                    return Enumerable.Repeat(ImmutableHashSet<int>.Empty.Add(curr.Gid), 1);
                }
                else
                {
                    return todo1.Values
                        .SelectMany(next =>
                            Clump(next, bad2, todo3))
                        .Select(s => s.Add(curr.Gid)); //THIS IS NO GOOD FOR ADDING ON CURRENT...
                }
            }
        }

        class Pending<T>
        {
            ImmutableSortedSet<T> _set;
            //could be dictionary keyed by gid...

            public Pending(IEnumerable<T> vals)
            {
                _set = vals.ToImmutableSortedSet();
            }
            
            public bool IsEmpty => _set.IsEmpty;

            public Pending<T> Pop(out T val)
            {
                val = _set[0];
                var set2 = _set.Remove(val);
                return new Pending<T>(set2);
            }
        }
        

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

                        var clumps = ClumpHits(_hits);
                            
                        return clumps.Aggregate(
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
                    });
                
                return Bounce(left, right, mV);
                
                IEnumerable<ImmutableHashSet<Group<N, V>>> ClumpHits(ImmutableHashSet<Group<N, V>> _hits)
                {
                    switch (_hits.Count)
                    {
                        case 0: 
                        case 1: return new[] { _hits };
                        default:
                        {
                            var head = _hits.First();
                            var tail = _hits.Remove(head);

                            //we're in the business of gathering others into our clump here, aren't we?
                            //so below, we find tail groups that can sit happily alongside us
                            //it'd be nice if each group were actually problematised here
                            //ie we've got our hit groups; we should now mask these to remove extraneous disjuncts (replacing each with empty group)
                            //now we need to sort them so that we ahve enumerable clumps to project across
                            
                            var tailIndies = tail
                                .Where(g => !g.Disjuncts.Contains(head.Gid))
                                .ToImmutableHashSet();

                            if (tailIndies.IsEmpty)
                            {
                                //what are we supposed to be doing here then?
                                //tailIndies should be part of same clump
                                //except for fact that they might not be independent of each other
                                //
                            }
                            else
                            {
                                //tailIndies themselves should be clumped?
                                //tailIndies should live in same clump as head!
                            }
                            
                            var tailClumps = ClumpHits(tailIndies)
                                .Select(set => set.Add(head))
                                .ToImmutableHashSet();

                            if (tailClumps.IsEmpty)
                            {
                                tailClumps = new[] { new[] { head }.ToImmutableHashSet() }.ToImmutableHashSet();
                            }

                            var tail2 = tailClumps
                                .SelectMany(set => set)
                                .Aggregate(tail, (ac, g) => ac.Remove(g));

                            return tailClumps
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
            => $"{{{string.Join(", ", _groups.Select(g => g.Value))}}}";

        #region Equality

        public bool Equals(Map<N, V> other)
        {
            var leftGroups = _groups.Values.OrderBy(g => g.GetHashCode());
            var rightGroups = other?._groups.Values.OrderBy(g => g.GetHashCode());
            return rightGroups?.SequenceEqual(leftGroups) ?? false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Map<N, V>) obj);
        }

        public override int GetHashCode()
            => _hash;

        #endregion
    }
}
