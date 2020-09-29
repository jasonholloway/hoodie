using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Hoodie.GroupMaps
{
    public abstract class GroupMap
    {
        public static Map<N, V> Lift<N, V>(IEnumerable<N> nodes, V val)
            => Map<N, V>.Lift(nodes, val);
    }

    public class Map<N, V> : IEquatable<Map<N, V>>, IComparable<Map<N, V>>
    {
        static long _nextId = 0;

        readonly long _id = Interlocked.Increment(ref _nextId);
        readonly int _nextGid = 0;
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
            _nextGid = gid;
            _groups = groups ?? ImmutableSortedDictionary<int, Group<N, V>>.Empty;
            _index = index ?? ImmutableSortedDictionary<N, ImmutableHashSet<int>>.Empty;
            _hash = _groups
                .Select(kv => kv.Value)
                .Aggregate(17, (ac, g) => (ac + g.GetHashCode() * 3) + 5);
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



        public Map<N, V> Crop(ISet<N> cropTo)
            => _groups.Values
                .Select(g => (
                    nodes: g.Nodes.Intersect(cropTo), 
                    val: g.Value
                    ))
                .Where(t => !t.nodes.IsEmpty)
                .Aggregate(
                    Empty,
                    (ac, t) => ac.Add(t.nodes, t.val));
                    
                    
        //I need to remember what I was doing basically, to get back on the track here
        //clumping was the issue, the desired mechanism was one to - what?
        //groups aren't allowed to overlap, there can only be one group in each position,
        //
        //so in bringing two groups together - the atomic action of my system - choices have to be made, flattening down to one choice from a set
        //or if not made eagerly, the choices must be maintained and explored and propagated till they whittle themselvese down
        //
        //we're expecting on output a conjunction of possibilities
        //this conjunction will then be used to bounce down the two graphs
        //
        //each input graphis in the first place a bundle of conjunctions
        //in combining two bundles of conjunctions, the crinkles should be ironed out, things should solidify
        //
        //implied that rhs is smaller than lhs; if rhs is in the first place a conjunction, if only implicitly, 
        //all conjunctions on the right must be reflected in the result
        //so we clump the rhs first off; in fact we must clump both sides
        //then multiply the conjunctions 
        //
        //that's the basic way to deal with conjunctions multiplied: each possiblity must be mated with each possibility,
        //then they must be filtered for impossibilities: that's the general approach
        //
        //firstly is a kind of lifting out of both sides
        //we only concern ourselvees with groups that impinge on our area
        //and we multiply together all disjuncts, squaring them
        //so now the idea is to zip together discjuntion-wise, instead of going per port
        
        
        

        public Disjunction<N, V> Hit(ISet<N> nodes)
        {
            var urClump = new Clump(this, 
                nodes.SelectMany(LookupIndexed)
                    .Distinct()
                    .Select(gid => _groups[gid])
                    .ToImmutableDictionary(
                        g => g.Gid,
                        g => new ClumpGroup(g, g.Disjuncts)
                        ));
            
            //off-graph disjuncts should be represented as empty sets
            //for below settling
            //^ THE TRUTH!!
            
            //everything is loaded in as a separate group
            //now we want to go about clumping them....
            //
            //this is different to approach above, whereby we have an unsettled clump up front
            //then gradually fracture it till no more fracturing can be done
            //
            //the other approach is to start from complete separation, and gradually tack on new choices, like
            //a brute force solving of a jigsaw, with every piece combination being tried in turn
            //but whether we like it or not, this is how we start off; even if all is housed in a single big clump,
            //it is essentially fractured, as we can't trust the nature of the container, can't infer anything
            //from the fact of its containment.
            //
            //so sticking it all in a clump is gaining us nothing? it lets us pose the problem as one that always has a clump as a subject
            //it's a clump, but a frangible clump - we grind it till the bits become solid, like a millstone on wheat
            //from here we divide and conquer, we hope; we choose a fracture then snap it
            //the problem is this fundamentally can't bring together, which was the original point of the clump, of course
            //
            //the overall map has links of disjunction; we are now in the opposite business of forming conjunctions
            //
            //quite simply, links need making between 
            //
            

            var clumps = Settle(new HashSet<Clump>(), urClump);
            
            return Disjunction.From(clumps.Select(c => c.ToMap().Crop(nodes)));
            

            IEnumerable<Clump> Settle(HashSet<Clump> seenBefore, Clump clump)
            {
                if (seenBefore.Contains(clump))
                {
                    return Enumerable.Empty<Clump>();
                }
                else
                {
                    seenBefore.Add(clump);
                }
                
                if(!clump.TryGetDisjunct(out var tup)) 
                {
                    return new[] {clump};
                }
                else
                {
                    var (lgid, rgid) = tup;
                    
                    var leftClump = clump.RemoveGroup(rgid);
                    var leftResults = Settle(seenBefore, leftClump);

                    var rightClump = clump.RemoveGroup(lgid);
                    var rightResults = Settle(seenBefore, rightClump);

                    return leftResults.Concat(rightResults);
                }
            }
        }

        
        class Clump : IEquatable<Clump>, IComparable<Clump>
        {
            static long _nextId = 0;

            readonly long _id = Interlocked.Increment(ref _nextId);
            readonly int _hash;

            public readonly Map<N, V> Map;
            public readonly ImmutableSortedDictionary<int, ClumpGroup> Groups;

            public Clump(Map<N, V> map, IDictionary<int, ClumpGroup> groups)
            {
                Map = map;
                Groups = groups.ToImmutableSortedDictionary();
                _hash = groups.Aggregate(0, (ac, kv) => (ac + (kv.Key + 1).GetHashCode() * 1379) + kv.Value.Disjuncts.Count);
            }

            public bool TryGetDisjunct(out (int, int) tup)
            {
                var found = Groups.Values
                    .SelectMany(cg => cg.Disjuncts.Select(did => Tuple.Create(cg.Group.Gid, did)))
                    .FirstOrDefault();

                if (found != null)
                {
                    tup = found.ToValueTuple();
                    return true;
                }
                else
                {
                    tup = default;
                    return false;
                }
            }

            public Clump RemoveGroup(int gid)
            {
                var groups2 = Groups.Remove(gid);

                var groups3 = groups2.Values.Aggregate(
                    groups2,
                    (ac, g) =>
                    {
                        var disjuncts2 = g.Disjuncts.Remove(gid);
                        if (!disjuncts2.Equals(g.Disjuncts))
                        {
                            return ac.SetItem(g.Group.Gid, new ClumpGroup(g.Group, disjuncts2));
                        }
                        else
                        {
                            return ac;
                        }
                    });
                
                return new Clump(Map, groups3);
            }

            public Map<N, V> ToMap()
                => Groups.Values.Aggregate(Empty,
                    (m, g) => m.Add(g.Group.Nodes, g.Group.Value));
            
            #region Equality, Comparable

            public override string ToString()
                => $"{{{string.Join(",", Groups.Values.Select(g => g.Group.Value))}}}";

            public bool Equals(Clump other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                if (_id == other._id) return true;
                if (_hash != other._hash) return false;
                if (Map._id != other.Map._id) return false;
                return Groups.Keys.SequenceEqual(other.Groups.Keys)
                    && Groups.Values.SelectMany(g => g.Disjuncts)
                        .SequenceEqual(other.Groups.Values.SelectMany(g => g.Disjuncts));
            }
            
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Clump) obj);
            }

            public int CompareTo(Clump other)
            {
                var hashComparison = _hash.CompareTo(other._hash);
                return hashComparison == 0 
                    ? _id.CompareTo(other._id)
                    : hashComparison;
            }

            public override int GetHashCode()
                => _hash;
            
            #endregion
        }

        class ClumpGroup
        {
            public readonly Group<N, V> Group;
            public readonly ImmutableHashSet<int> Disjuncts;

            public ClumpGroup(Group<N, V> @group, ImmutableHashSet<int> disjuncts)
            {
                Group = @group;
                Disjuncts = disjuncts;
            }
        }
        
        

        public Disjunction<N, V> _Hit(ISet<N> nodes)
        {
            var todo = nodes
                .SelectMany(n => this[n])
                .ToImmutableDictionary(g => g.Gid);

            var clumps = todo.Values
                .SelectMany(g => Clump(g, ImmutableHashSet<int>.Empty, todo));
            
            //if all todos have the same disjunct, then we need to add an empty map
            //could be done more efficiently as part of recursion
            
            var dids = todo.Values.SelectMany(g => g.Disjuncts).ToImmutableHashSet();
            if (dids.Any(did => todo.Values.All(g => g.Disjuncts.Contains(did))))
            {
                clumps = clumps.Concat(new[] { Empty });
            }

            return Disjunction.From(clumps);
            
            //tho clump below visits groups multiple times already
            //above has a different journey - wanna go through groups
            //if there's a single group, then there won't be an empty added
            

            IEnumerable<Map<N, V>> Clump(Group<N, V> curr, ImmutableHashSet<int> bad1, ImmutableDictionary<int, Group<N, V>> todo1)
            {
                var bad2 = curr.Disjuncts.Aggregate(bad1, (ac, did) => ac.Add(did));
                
                var todo2 = todo1
                    .Remove(curr.Gid)
                    .RemoveRange(bad2);

                if (todo2.IsEmpty)
                {
                    yield return Empty.Add(curr.Nodes.Intersect(nodes), curr.Value);
                }
                else
                {
                    foreach (var innerClump in todo2.Values.SelectMany(next => Clump(next, bad2, todo2)))
                    {
                        yield return innerClump.Add(curr.Nodes.Intersect(nodes), curr.Value);
                    }
                }
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
                    var found2 = found.AddDisjunct(_nextGid);
                    var ac2 = ac.SetItem(did, found2);
                    return ac2;
                });

            return new Map<N, V>(
                _nextGid + 1,
                groups2.Add(_nextGid, new Group<N, V>(_nextGid, _newNodes, disjuncts, newVal)),
                _newNodes.Aggregate(
                    _index,
                    (ac, n) => ac.TryGetValue(n, out var indexed) 
                        ? ac.SetItem(n, indexed.Add(_nextGid))
                        : ac.Add(n, ImmutableHashSet<int>.Empty.Add(_nextGid))
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
                
                return new Map<N, V>(_nextGid, groups3, index);
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

        public int CompareTo(Map<N, V> other)
        {
            var hashComparison = _hash.CompareTo(other._hash);
            return hashComparison == 0 
                ? _id.CompareTo(other._id)
                : hashComparison;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            if (GetHashCode() != obj.GetHashCode()) return false;
            return Equals((Map<N, V>) obj);
        }

        public override int GetHashCode()
            => _hash;

        #endregion
    }
}
