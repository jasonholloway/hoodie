using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using static Hoodie.GroupMaps.Tests.GroupMapTestHelpers;

namespace Hoodie.GroupMaps.Tests
{
    public class MapIndexAndDisjunctTests
    {
        [Test]
        public void SimpleMap_IsIndexed()
        {
            var map = Map(1, 'A');
            var found = map[1];

            Assert.That(found.Count(), Is.EqualTo(1));
            Assert.That(found.Single().Nodes, Is.EquivalentTo(new[] { 1 }));
            Assert.That(found.Single().Value, Is.EqualTo((Sym)'A'));
        }
        
        [Test]
        public void SimpleMap_HasNoDisjuncts()
        {
            var map = Map(1, 'A');
            var groups = map[1];
            Assert.That(groups.Count(), Is.EqualTo(1));

            var group = groups.Single();
            Assert.That(group.Disjuncts, Is.Empty);
        }
        
        [Test]
        public void SummedMap_IsIndexed()
        {
            var map = Map(1, 'A').Add(Map(2, 'B'));

            var found1 = map[1];
            Assert.That(found1.Count(), Is.EqualTo(1));
            
            var found2 = map[2];
            Assert.That(found2.Count(), Is.EqualTo(1));
        }
        
        [Test]
        public void SummedMap_HasDisjuncts()
        {
            var map = Map(1, 'A').Add(Map(1, 'B'));
            
            var groups = map[1];
            Assert.That(groups.Count(), Is.EqualTo(2));
            
            var group1 = groups.ElementAt(0);
            Assert.That(group1.Disjuncts.Count(), Is.EqualTo(1));

            var group2 = groups.ElementAt(1);
            Assert.That(group2.Disjuncts.Count(), Is.EqualTo(1));
        }
    }

    public class MapCombinationTests
    {
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
        public void Combine_Overlaps()
            => Test(@"
                A . . . AB
                A * B = AB
                . . B . AB
                ");

        [Test]
        public void Combine_NonOverlaps()
            => Test(@"
                A . . . A
                A * . = A
                . . B . B
                ");

        [Test]
        public void Combine_Disjuncts()
            => Test(@"
                A B . C . AC BC
                A . * . = AC .
                . B . C . AC BC
            ");

        [Test]
        public void Combine_ComplexDisjuncts1()
            => Test(@"
                A . . D . ACD .
                A B * . = ACD B
                . B . . . .   B
                . C . D . ACD .
            ");
        
        [Test]
        public void Combine_ComplexDisjuncts2()
            => Test(@"
                A . . . D . ACD BD
                A B . * . = ACD BD
                . B . . . . .   BD 
                . B C . D . ACD BD
            ");
        
        [Test]
        public void Combine_ComplexDisjuncts3()
            => Test(@"
                A . . . D . ABD ACD 
                . B . * . = ABD .
                . B C . D . ABD ACD
            ");
        
        [Test]
        public void Combine_DuplexDisjuncts1()
            => Test(@"
                . . . C . .  AC 
                A * B C = AB AC
                A . . . . AB AC
            ");
        
        [Test]
        public void Combine_DuplexDisjuncts2()
            => Test(@"
                A . . . D . A AD .  BD 
                . B * C D = C AD BC BD
                A B . . . . A AD BC BD
            ");
    }

    public class MapTests
    {
        [Test]
        public void Group_Equality()
        {
            var g1 = Group((1, 2), 'A');
            var g2 = Group((1, 2), 'A');
            Assert.That(g2, Is.EqualTo(g1));

            var h1 = ImmutableHashSet<SimpleGroup<int, Sym>>
                .Empty.Add(g1);
            var h2 = ImmutableHashSet<SimpleGroup<int, Sym>>
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
        public void AddingRemoving()
        {
            var m1 = EmptyMap
                .Add(Map((1, 2), 'A'));

            var m2 = EmptyMap
                .Add(Map((2, 3), 'B'))
                .Add(Map((1, 2), 'A'));

            var m3 = m2
                .Remove(m2[3].First());

            Assert.That(m3, Is.EqualTo(m1).Using(MapComp));
        }

        [Test]
        public void AddingRemoving_Indices()
        {
            var map1 = Map((1, 2), 'A');
            var map2 = Map((2, 3), 'B');

            var map3 = EmptyMap
                .Add(map1)
                .Add(map2);

            Assert.Multiple(() =>
            {
                Assert.That(map3[1].Simple(), Is.EquivalentTo(new[] { Group((1, 2), 'A') }));
                Assert.That(map3[2].Simple(), Is.EquivalentTo(new[] { Group((1, 2), 'A'), Group((2, 3), 'B') }));
                Assert.That(map3[3].Simple(), Is.EquivalentTo(new[] { Group((2, 3), 'B') }));
            });

            var map4 = map3.Remove(map1[1].First());

            Assert.Multiple(() =>
            {
                Assert.That(map4[1], Is.Empty);
                Assert.That(map4[2].Simple(), Is.EquivalentTo(new[] { Group((2, 3), 'B') }));
                Assert.That(map4[3].Simple(), Is.EquivalentTo(new[] { Group((2, 3), 'B') }));
            });
        }

        [Test]
        public void SimpleEquality()
            => Test(@"
                 A . A
                 A = A
             ");

        [Test]
        public void Equality_OfDisjuncts()
            => Test(@"
                 A B . B A
                 A B = B A
             ");

        [Test]
        public void Equality_OfDisjuncts2()
            => Test(@"
                 A . . A
                 . B = B
                 C D . C D
             ");


        [Test]
        public void BuildAMap()
        {
            var map = BuildMap(@"
                A B
                A .
                . B
            ");

            Assert.Multiple(() =>
            {
                Assert.That(map[1].Simple(), Is.EquivalentTo(new[]
                {
                    Group((1, 2), 'A'),
                    Group((1, 3), 'B')
                }));

                Assert.That(map[2].Simple(), Is.EquivalentTo(new[]
                {
                    Group((1, 2), 'A'),
                }));

                Assert.That(map[3].Simple, Is.EquivalentTo(new[]
                {
                    Group((1, 3), 'B')
                }));
            });
        }
    }

    public static class TestExtensions
    {
        public static SimpleGroup<int, Sym> Simple(this Group<int, Sym> group)
            => SimpleGroup.From(group.Nodes, group.Value);

        public static IEnumerable<SimpleGroup<int, Sym>> Simple(this IEnumerable<Group<int, Sym>> groups)
            => groups.Select(g => g.Simple());

    }
}