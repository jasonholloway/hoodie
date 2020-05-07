using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Hoodie.GroupMaps.Tests
{
    public class GroupMapTests
    {
        [Test]
        public void GroupMap_Equality()
        {
            var map1 = Map((1, 2), "woo");
            var map2 = Map((1, 2), "woo");
            Assert.That(map2, Is.EqualTo(map1));
        }
        
        [Test]
        public void EmptyEmpty_AndEmpty()
        {
            var map1 = EmptyMap;
            var map2 = EmptyMap;

            var combined = map1.Combine(map2);
            Assert.That(combined, Is.EqualTo(EmptyMap));
        }
        
        [Test]
        public void EmptyOne_AndOne()
        {
            var map1 = EmptyMap;
            var map2 = Map(1, "one");

            var combined = map1.Combine(map2);
            Assert.That(combined, Is.EqualTo(map2));
        }
        
        [Test]
        public void OneEmpty_AndOne()
        {
            var map1 = Map(1, "one");
            var map2 = EmptyMap;

            var combined = map1.Combine(map2);
            Assert.That(combined, Is.EqualTo(map1));
        }

        [Test]
        public void AddingRemoving()
        {
            var m1 = EmptyMap
                .Add(Group.From(new[] {1, 2}, "one"));
            
            var m2 = EmptyMap
                .Add(Group.From(new[] {2, 3}, "two"))
                .Add(Group.From(new[] { 1, 2 }, "one"))
                .Remove(Group.From(new[] { 2, 3 }, "two"));

            Assert.That(m2, Is.EqualTo(m1));
        }

        [Test]
        public void AddingRemoving_Indices()
        {
            var group1 = Group.From(new[] {1, 2}, "one");
            var group2 = Group.From(new[] {2, 3}, "two"); 
            
            var map = EmptyMap
                .Add(group1)
                .Add(group2);

            Assert.Multiple(() =>
            {
                Assert.That(map[1], Is.EquivalentTo(new[] { group1 }));
                Assert.That(map[2], Is.EquivalentTo(new[] { group1, group2 }));
                Assert.That(map[3], Is.EquivalentTo(new[] { group2 }));
            });

            var map2 = map.Remove(group1);
            
            Assert.Multiple(() =>
            {
                Assert.That(map2[1], Is.Empty);
                Assert.That(map2[2], Is.EquivalentTo(new[] { group2 }));
                Assert.That(map2[3], Is.EquivalentTo(new[] { group2 }));
            });
        }

        [Test]
        public void SimpleEquality()
            => Interpret(@"
                 A . A
                 A = A
             ");
        
        [Test]
        public void Equality_OfDisjuncts()
            => Interpret(@"
                 A B . B A
                 A B = B A
             ");
        
        [Test]
        public void Equality_OfDisjuncts2()
            => Interpret(@"
                 A . . A
                 . B = B
                 C D . C D
             ");
        
        [Test]
        public void Combine_Overlaps()
            => Interpret(@"
                A . . . AB
                A * B = AB
                . . B . AB
                ");

        [Test]
        public void Combine_Disjuncts()
            => Interpret(@"
                A B . C . AC BC
                A . * . = AC .
                . B . C . .  BC
            ");
        

        static GroupMap<int, string> EmptyMap = GroupMap<int, string>.Empty;
        
        static GroupMap<int, string> Map(params (int[], string)[] groups)
            => groups.Aggregate(
                EmptyMap,
                (m, t) => m.Add(Group.From(t.Item1, t.Item2)));

        static GroupMap<int, string> Map(string val)
            => Map((new int[0], val));
        
        static GroupMap<int, string> Map(int node, string val)
            => Map((new[] { node }, val));
        
        static GroupMap<int, string> Map((int, int) nodes, string val)
            => Map((new[] { nodes.Item1, nodes.Item2 }, val));
        
        static GroupMap<int, string> Map((int, int, int) nodes, string val)
            => Map((new[] { nodes.Item1, nodes.Item2, nodes.Item3 }, val));
        
        
        void Interpret(string code)
        {
            var matches = Regex
                .Matches(code, @"^(?: +([\w\.\*\+\=]+))+", RegexOptions.Multiline);
                
            var slices = matches
                .SelectMany((m, y) => m.Groups[1].Captures.Select((c, x) => (y, x, c.Value)))
                .GroupBy(t => t.x, t => (t.y, t.Value))
                .Select(g => g.GroupBy(t => t.Value)
                                .Where(gg => gg.Key != ".")
                                .Select(gg => Group.From(gg.Select(x => x.y), gg.Key)))
                .SelectMany(slice => slice switch 
                {
                    _ when slice.Any(g => g.Value == "*")
                        => new object[] { "*" },
                    _ when slice.Any(g => g.Value == "=")
                        => new object[] { "=" },
                    _ 
                        => (IEnumerable<object>)slice
                });

            var tokens = new Queue<object>(slices);
            var map = GroupMap<int, string>.Empty;
            
            while (tokens.Any())
            {
                _ = ReadMap(ref map) 
                    || ReadMulti(ref map)
                    || ReadEquals(ref map);
            }

            bool ReadMap(ref GroupMap<int, string> map)
            {
                if (!tokens.Any()) return false;
                if (tokens.Peek() is Group<int, string> group)
                {
                    tokens.Dequeue();
                    map = map.Add(group);
                    ReadMap(ref map);
                    return true;
                }

                return false;
            }
            
            bool ReadMulti(ref GroupMap<int, string> left)
            {
                if (!tokens.Any()) return false;
                switch (tokens.Peek())
                {
                    case "*":
                        tokens.Dequeue();
                        
                        var right = GroupMap<int, string>.Empty;
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

            bool ReadEquals(ref GroupMap<int, string> left)
            {
                if (!tokens.Any()) return false;
                switch (tokens.Peek())
                {
                    case "=":
                        tokens.Dequeue();

                        var right = GroupMap<int, string>.Empty;
                        if (!ReadMap(ref right))
                        {
                            throw new InvalidOperationException("'*' needs groups to its right!");
                        }
                        
                        Assert.That(left, Is.EqualTo(right));
                        return true;
                    
                    default:
                        return false;
                }
            }
        }

    }
}