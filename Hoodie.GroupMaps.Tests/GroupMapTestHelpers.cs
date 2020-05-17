using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Hoodie.GroupMaps.Tests
{
    public static class GroupMapTestHelpers 
    {
        public static Map<int, Sym> EmptyMap = Map<int, Sym>.Empty;
        
        public static Map<int, Sym> Map(params (int[], Sym)[] groups)
            => groups.Aggregate(
                EmptyMap,
                (m, t) => m.Add(GroupMap.Lift(t.Item1, t.Item2)));

        public static Map<int, Sym> Map(Sym sym)
            => Map((new int[0], sym));
        
        public static Map<int, Sym> Map(int node, Sym val)
            => Map((new[] { node }, val));
        
        public static Map<int, Sym> Map((int, int) nodes, Sym val)
            => Map((new[] { nodes.Item1, nodes.Item2 }, val));
        
        public static Map<int, Sym> Map((int, int, int) nodes, Sym val)
            => Map((new[] { nodes.Item1, nodes.Item2, nodes.Item3 }, val));
        

        public static SimpleGroup<int, Sym> Group(int node, Sym sym)
            => SimpleGroup.From(new[] {node}, sym);
        
        public static SimpleGroup<int, Sym> Group((int, int) nodes, Sym sym)
            => SimpleGroup.From(new[] {nodes.Item1, nodes.Item2}, sym);
        
        public static SimpleGroup<int, Sym> Group((int, int, int) nodes, Sym sym)
            => SimpleGroup.From(new[] {nodes.Item1, nodes.Item2, nodes.Item3}, sym);

        public static Map<int, Sym> BuildMap(string code)
            => (Map<int, Sym>)Test(code);
        
        public static object Test(string code)
        {
            var matches = Regex
                .Matches(code, @"^(?: +([\w\.\*\+\=]+))+", RegexOptions.Multiline);
                
            var slices = matches
                .SelectMany((m, y) => m.Groups[1].Captures.Select((c, x) => (y: y + 1, x, v: Sym.From(c.Value))))
                .GroupBy(t => t.x, t => (t.y, t.v))
                .Select(g => g.GroupBy(t => t.v)
                    .Where(gg => gg.Key != '.')
                    .Select(gg => SimpleGroup.From(gg.Select(x => x.y), gg.Key)))
                .SelectMany(slice => slice switch 
                {
                    _ when slice.Any(g => g.Value == '*')
                    => new object[] { "*" },
                    _ when slice.Any(g => g.Value == '=')
                    => new object[] { "=" },
                    _ 
                    => (IEnumerable<object>)slice
                });

            var tokens = new Queue<object>(slices);
            var map = Map<int, Sym>.Empty;
            
            while (tokens.Any())
            {
                _ = ReadMap(ref map) 
                    || ReadMulti(ref map)
                    || ReadEquals(ref map);
            }

            return map;

            bool ReadMap(ref Map<int, Sym> map)
            {
                if (!tokens.Any()) return false;
                if (tokens.Peek() is SimpleGroup<int, Sym> group)
                {
                    tokens.Dequeue();
                    map = map.Add(GroupMap.Lift(group.Nodes, group.Value));
                    ReadMap(ref map);
                    return true;
                }

                return false;
            }
            
            bool ReadMulti(ref Map<int, Sym> left)
            {
                if (!tokens.Any()) return false;
                switch (tokens.Peek())
                {
                    case "*":
                        tokens.Dequeue();
                        
                        var right = Map<int, Sym>.Empty;
                        if (!ReadMap(ref right))
                        {
                            throw new InvalidOperationException("'*' needs groups to its right!");
                        }

                        left = left.Combine(right);
                        return true;
                    
                    default:
                        return false;
                }
            }

            bool ReadEquals(ref Map<int, Sym> left)
            {
                if (!tokens.Any()) return false;
                switch (tokens.Peek())
                {
                    case "=":
                        tokens.Dequeue();

                        var right = Map<int, Sym>.Empty;
                        if (!ReadMap(ref right))
                        {
                            throw new InvalidOperationException("'*' needs groups to its right!");
                        }
                        
                        Assert.That(left, Is.EqualTo(right).Using(MapComp));
                        return true;
                    
                    default:
                        return false;
                }
            }
        }

        internal static readonly Comparison<Map<int, Sym>> MapComp = (m1, m2) =>
        {
            var groups1 = m1.Groups.Select(g => g.Item2.Simple()).ToHashSet();
            var groups2 = m2.Groups.Select(g => g.Item2.Simple()).ToHashSet();
            return groups1.SetEquals(groups2) ? 0 : -1;
        };

    }
}