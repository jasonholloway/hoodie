using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            var set = ImmutableHashSet<Map<int, Sym>>.Empty;
            set = set.Add(Read(node.Left).As<Map<int, Sym>>());

            var right = Read(node.Right);
            return right switch
            {
                Map<int, Sym> map => set.Add(map),
                ISet<Map<int, Sym>> set2 => set.Union(set2),
                _ => throw new Exception($"Strange value read: {right}"),
            };
        }

        static object _Read(CombinationNode node)
        {
            var left = Read(node.Left).As<Map<int, Sym>>();
            var right = Read(node.Right).As<Map<int, Sym>>();

            return left.Combine(right);
        }
        
        static object _Read(HitNode node)
        {
            var left = Read(node.Left).As<Map<int, Sym>>();
            var right = Read(node.Right).As<ISet<Map<int, Sym>>>();
            
            return new Action(() =>
            {
                var result = left.Hit(node.Nodes);
                Assert.That(result, Is.EquivalentTo(right).Using(MapComp));
            });
        }
        
        static object _Read(EqualsNode node)
        {
            var left = Read(node.Left).As<Map<int, Sym>>();
            var right = Read(node.Right).As<Map<int, Sym>>();
            
            return new Action(() =>
            {
                Assert.That(left, Is.EqualTo(right).Using(MapComp));
            });
        }
            
        public static readonly Comparison<Map<int, Sym>> MapComp = (m1, m2) =>
        {
            var groups1 = m1.Groups.Select(g => g.Simple()).ToHashSet();
            var groups2 = m2.Groups.Select(g => g.Simple()).ToHashSet();
            return groups1.SetEquals(groups2) ? 0 : -1;
        };
    }
}