using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Hoodie.GroupMaps.Tests
{
    public class SymTests
    {
        [Test]
        public void Equality()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Sym.From('A'), Is.EqualTo(Sym.From('A')));
                Assert.That(Sym.From("HAMSTER"), Is.EqualTo(Sym.From("HAMSTER")));
            });
        }

        [Test]
        public void HashCodes()
        {
            var syms = new Sym[]
            {
                'A', 'A', 'A'
            };

            var hashes = syms.Select(s => s.GetHashCode()).ToHashSet();
            Assert.That(hashes, Has.Count.EqualTo(1));
        }
    }
    
    
    public class GroupMapTests
    {
        [Test]
        public void Group_Equality()
        {
            var g1 = Group((1, 2), 'A');
            var g2 = Group((1, 2), 'A');
            Assert.That(g2, Is.EqualTo(g1));

            var h1 = ImmutableHashSet<Group<int, Sym>>
                        .Empty.Add(g1);
            var h2 = ImmutableHashSet<Group<int, Sym>>
                        .Empty.Add(g1);
            
            Assert.That(h1, Is.EqualTo(h2));
            Assert.That(h1.SetEquals(h2));
        }
        
        [Test]
        public void GroupMap_Equality1()
        {
            var map1 = Map(1, 'A');
            var map2 = Map(1, 'A');
            Assert.That(map2, Is.EqualTo(map1));
        }
        
        [Test]
        public void GroupMap_Equality2()
        {
            var map1 = Map((1, 2), 'A');
            var map2 = Map((1, 2), 'A');
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
            var map2 = Map(1, 'A');

            var combined = map1.Combine(map2);
            Assert.That(combined, Is.EqualTo(map2));
        }
        
        [Test]
        public void OneEmpty_AndOne()
        {
            var map1 = Map(1, 'A');
            var map2 = EmptyMap;

            var combined = map1.Combine(map2);
            Assert.That(combined, Is.EqualTo(map1));
        }

        [Test]
        public void AddingRemoving()
        {
            var m1 = EmptyMap
                .Add(Group((1, 2), 'A'));

            var m2 = EmptyMap
                .Add(Group((2, 3), 'B'))
                .Add(Group((1, 2), 'A'))
                .Remove(Group((2, 3), 'B'));

            Assert.That(m2, Is.EqualTo(m1));
        }

        [Test]
        public void AddingRemoving_Indices()
        {
            var group1 = Group((1, 2), 'A');
            var group2 = Group((2, 3), 'B');
            
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
        public void Combine_NonOverlaps()
            => Interpret(@"
                A . . . A
                A * . = A
                . . B . B
                ");

        [Test]
        public void Combine_Disjuncts()
            => Interpret(@"
                A B . C . AC BC
                A . * . = AC .
                . B . C . .  BC
            ");

        [Test]
        public void BuildAMap()
        {
            var map = BuildMap(@"
                A B
                A .
                . B
            ");
        }
        

        static GroupMap<int, Sym> EmptyMap = GroupMap<int, Sym>.Empty;
        
        static GroupMap<int, Sym> Map(params (int[], Sym)[] groups)
            => groups.Aggregate(
                EmptyMap,
                (m, t) => m.Add(GroupMaps.Group.From(t.Item1, t.Item2)));

        static GroupMap<int, Sym> Map(Sym sym)
            => Map((new int[0], sym));
        
        static GroupMap<int, Sym> Map(int node, Sym val)
            => Map((new[] { node }, val));
        
        static GroupMap<int, Sym> Map((int, int) nodes, Sym val)
            => Map((new[] { nodes.Item1, nodes.Item2 }, val));
        
        static GroupMap<int, Sym> Map((int, int, int) nodes, Sym val)
            => Map((new[] { nodes.Item1, nodes.Item2, nodes.Item3 }, val));
        

        static Group<int, Sym> Group(int node, Sym sym)
            => GroupMaps.Group.From(new[] {node}, sym);
        
        static Group<int, Sym> Group((int, int) nodes, Sym sym)
            => GroupMaps.Group.From(new[] {nodes.Item1, nodes.Item2}, sym);
        
        static Group<int, Sym> Group((int, int, int) nodes, Sym sym)
            => GroupMaps.Group.From(new[] {nodes.Item1, nodes.Item2, nodes.Item3}, sym);

        GroupMap<int, Sym> BuildMap(string code)
            => (GroupMap<int, Sym>)Interpret(code);
        
        object Interpret(string code)
        {
            var matches = Regex
                .Matches(code, @"^(?: +([\w\.\*\+\=]+))+", RegexOptions.Multiline);
                
            var slices = matches
                .SelectMany((m, y) => m.Groups[1].Captures.Select((c, x) => (y: y + 1, x, v: Sym.From(c.Value))))
                .GroupBy(t => t.x, t => (t.y, t.v))
                .Select(g => g.GroupBy(t => t.v)
                                .Where(gg => gg.Key != '.')
                                .Select(gg => GroupMaps.Group.From(gg.Select(x => x.y), gg.Key)))
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
            var map = GroupMap<int, Sym>.Empty;
            
            while (tokens.Any())
            {
                _ = ReadMap(ref map) 
                    || ReadMulti(ref map)
                    || ReadEquals(ref map);
            }

            return map;

            bool ReadMap(ref GroupMap<int, Sym> map)
            {
                if (!tokens.Any()) return false;
                if (tokens.Peek() is Group<int, Sym> group)
                {
                    tokens.Dequeue();
                    map = map.Add(group);
                    ReadMap(ref map);
                    return true;
                }

                return false;
            }
            
            bool ReadMulti(ref GroupMap<int, Sym> left)
            {
                if (!tokens.Any()) return false;
                switch (tokens.Peek())
                {
                    case "*":
                        tokens.Dequeue();
                        
                        var right = GroupMap<int, Sym>.Empty;
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

            bool ReadEquals(ref GroupMap<int, Sym> left)
            {
                if (!tokens.Any()) return false;
                switch (tokens.Peek())
                {
                    case "=":
                        tokens.Dequeue();

                        var right = GroupMap<int, Sym>.Empty;
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