using NUnit.Framework;

namespace Hoodie.GroupMaps.Tests
{
    public class GroupMapTests
    {
        [Test]
        public void GroupMap_EmptyEqualsEmpty()
        {
            var map1 = GroupMap<int, string>.Empty;
            var map2 = GroupMap<int, string>.Empty;
            Assert.That(map2, Is.EqualTo(map1));
        }
        
        [Test]
        public void GroupMap_Equality()
        {
            var map1 = GroupMap.From(new[] { 1, 2 }, "woo");
            var map2 = GroupMap.From(new[] { 1, 2 }, "woo");
            Assert.That(map2, Is.EqualTo(map1));
        }
        
        [Test]
        public void EmptyEmpty_AndEmpty()
        {
            var map1 = GroupMap<int, string>.Empty;
            var map2 = GroupMap<int, string>.Empty;

            var combined = map1.Combine(map2);
            Assert.That(combined, Is.EqualTo(GroupMap<int, string>.Empty));
        }
        
        [Test]
        public void EmptyOne_AndOne()
        {
            var map1 = GroupMap<int, string>.Empty;
            var map2 = GroupMap.From(new[] { 1 }, "one");

            var combined = map1.Combine(map2);
            Assert.That(combined, Is.EqualTo(map2));
        }
        
        [Test]
        public void OneEmpty_AndOne()
        {
            var map1 = GroupMap.From(new[] { 1 }, "one");
            var map2 = GroupMap<int, string>.Empty;

            var combined = map1.Combine(map2);
            Assert.That(combined, Is.EqualTo(map1));
        }

        [Test]
        public void AddingRemoving()
        {
            var empty = GroupMap<int, string>.Empty;
            
            var m1 = empty
                .Add(Group.From(new[] {1, 2}, "one"));
            
            var m2 = empty
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
            
            var map = GroupMap<int, string>.Empty
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
        public void CombiningOverlappingGroups()
        {
            var map1 = GroupMap.From(new[] {1, 2}, "one");
            var map2 = GroupMap.From(new[] {2, 3}, "two");

            var combined = map1.Combine(map2);
            Assert.That(combined, Is.EqualTo(
                GroupMap.From(new[] {1, 2, 3}, "onetwo")));
        }
    }
}