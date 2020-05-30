using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Hoodie.GroupMaps.Tests.MapLang
{
    public static class Runner
    {
        public static void Test(string code)
            => Run<Action>(code)();
        
        public static object Run(string code)
        {
            var slices = Slicer.Slice(code);
            var _node = Parser.Parse(slices);
            return Read(_node);
        }
        
        public static T Run<T>(string code)
            => (T) Run(code);
        

        static object Read(Node node)
            => _Read((dynamic)node);

        static object _Read(GridNode node)
        {
            var groups = node.Head
                .Select((v, y) => (y: y + 1, v))
                .Where(t => t.v != "")
                .GroupBy(t => t.v)
                .Select(g => SimpleGroup.From(g.Select(t => t.y), Sym.From(g.Key)))
                .ToArray();

            var map = groups.Aggregate(
                Map<int, Sym>.Empty,
                (m, g) => m.Add(GroupMap.Lift(g.Nodes, g.Value)));

            if (node.Tail != null)
            {
                var right = Read(node.Tail).As<Map<int, Sym>>();
                map = map.Add(right);
            }

            return map;
        }

        static object _Read(DisjunctionNode node)
        {
            var left = Read(node.Left).As<Map<int, Sym>>();

            return Read(node.Right) switch
            {
                Map<int, Sym> map => new Disjunction<int, Sym>(new[] {left, map}),
                ISet<Map<int, Sym>> set => new Disjunction<int, Sym>(new[] { left }.Concat(set)),
                Disjunction<int, Sym> d => new Disjunction<int, Sym>(d.Disjuncts.Add(left)),
                _ => throw new Exception("bad value")
            };
        }

        static object _Read(CombinationNode node)
        {
            var left = Read(node.Left).As<Map<int, Sym>>();
            var right = Read(node.Right).As<Map<int, Sym>>();

            return left.Combine(right);
        }
        
        static object _Read(CropNode node)
        {
            var left = Read(node.Left).As<Map<int, Sym>>();
            var right = Read(node.Right).As<Map<int, Sym>>();
            
            return new Action(() =>
            {
                var result = left.Crop(node.Nodes);
                Assert.That(result, Is.EqualTo(right)
                    .Using(MapEqualityComparer.Instance));
            });
        }
        
        static object _Read(HitNode node)
        {
            var left = Read(node.Left).As<Map<int, Sym>>();
            var right = Read(node.Right).AsDisjunction();
            
            return new Action(() =>
            {
                var result = left.Hit(node.Nodes);
                Assert.That(result, Is.EqualTo(right)
                    .Using(DisjunctionEqualityComparer.Instance));
            });
        }
        
        static object _Read(EqualsNode node)
        {
            var left = Read(node.Left).As<Map<int, Sym>>();
            var right = Read(node.Right).As<Map<int, Sym>>();
            
            return new Action(() =>
            {
                Assert.That(left, Is.EqualTo(right)
                    .Using(MapEqualityComparer.Instance));
            });
        }
        
        static object _Read(InequalsNode node)
        {
            var left = Read(node.Left).As<Map<int, Sym>>();
            var right = Read(node.Right).As<Map<int, Sym>>();
            
            return new Action(() =>
            {
                Assert.That(left, Is.Not.EqualTo(right));
            });
        }



        public class GroupEqualityComparer : IEqualityComparer<Group<int, Sym>>
        {
            public static GroupEqualityComparer Instance = new GroupEqualityComparer();
            
            public bool Equals(Group<int, Sym> g1, Group<int, Sym> g2)
                => (g1 == null && g2 == null)
                || (g1 != null && g2 != null
                    && g1.Nodes.SetEquals(g2.Nodes)
                    && g1.Value.Equals(g2.Value));

            public int GetHashCode(Group<int, Sym> g)
                => g.Nodes.Aggregate(7, (ac, n) => ac + n.GetHashCode() * 7 + 5);
        }
        
        public class MapEqualityComparer : IEqualityComparer<Map<int, Sym>>
        {
            public static MapEqualityComparer Instance = new MapEqualityComparer();
            
            public bool Equals(Map<int, Sym> m1, Map<int, Sym> m2)
                => (m1 == null && m2 == null) 
                || (m1 != null && m2 != null
                    && GetHashCode(m1) == GetHashCode(m2)
                    && m1.Groups.ToHashSet(new GroupEqualityComparer())
                        .SetEquals(m2.Groups));

            public int GetHashCode(Map<int, Sym> m)
                => m.Groups.Aggregate(13, (ac, g) => 
                    ac + GroupEqualityComparer.Instance.GetHashCode(g) * 3 + 1);
        }

        public class DisjunctionEqualityComparer : IEqualityComparer<Disjunction<int, Sym>>
        {
            public static DisjunctionEqualityComparer Instance = new DisjunctionEqualityComparer();

            public bool Equals(Disjunction<int, Sym> d1, Disjunction<int, Sym> d2)
                => (d1 == null && d2 == null)
                || (d1 != null && d2 != null
                    && GetHashCode(d1) == GetHashCode(d2)
                    && d1.Disjuncts.ToHashSet(MapEqualityComparer.Instance)
                        .SetEquals(d2.Disjuncts));

            public int GetHashCode(Disjunction<int, Sym> d)
                => d.Disjuncts.Aggregate(17, (ac, m) =>
                    ac + MapEqualityComparer.Instance.GetHashCode(m) * 7 + 3);
        }
    }
}