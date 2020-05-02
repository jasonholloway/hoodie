using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NUnit.Framework;

namespace Hoodie.Tests
{
    public class GroupMapTests
    {
        [Test]
        public void HashSetEquality()
        {
            var set1 = ImmutableHashSet<int>.Empty.Add(1).Add(2);
            var set2 = ImmutableHashSet<int>.Empty.Add(1).Add(2);
            Assert.That(set2, Is.EqualTo(set1));
        }
        
        [Test]
        public void GroupEquality()
        {
            var group1 = Group.From(new[] { 1, 2 }, "woo");
            var group2 = Group.From(new[] { 1, 2 }, "woo");
            Assert.That(group1, Is.EqualTo(group2));
            Assert.That(group1.Equals(group2), Is.True);
            Assert.That(EqualityComparer<Group<int, string>>.Default.Equals(group1, group2), Is.True);
        }
        
        [Test]
        public void GroupHashEquality()
        {
            var group1 = Group.From(new[] { 1, 2 }, "woo");
            var group2 = Group.From(new[] { 1, 2 }, "woo");
            Assert.That(group1.GetHashCode(), Is.EqualTo(group2.GetHashCode()));
        }
        
        [Test]
        public void GroupInequality1()
        {
            var group1 = Group.From(new[] { 1, 2 }, "woo!");
            var group2 = Group.From(new[] { 1, 2 }, "woo");
            Assert.That(group1, Is.Not.EqualTo(group2));
        }
        
        [Test]
        public void GroupInequality2()
        {
            var group1 = Group.From(new[] { 1, 2 }, "woo");
            var group2 = Group.From(new[] { 1, 2, 3 }, "woo");
            Assert.That(group1, Is.Not.EqualTo(group2));
        }

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
            var map1 = GroupMap<int, string>.Empty.Add(Group.From(new[] { 1, 2 }, "woo"));
            var map2 = GroupMap<int, string>.Empty.Add(Group.From(new[] { 1, 2 }, "woo"));
            Assert.That(map2, Is.EqualTo(map1));
        }
        
        
        
        
        [Test]
        public void EmptyEmpty_AndEmpty()
        {
            var map1 = GroupMap<int, string>.Empty;
            var map2 = GroupMap<int, string>.Empty;

            var combined = GroupMap.Combine(map1, map2);
            Assert.That(combined, Is.EqualTo(GroupMap<int, string>.Empty));
        }
        
        [Test]
        public void EmptyOne_AndOne()
        {
            var map1 = GroupMap<int, string>.Empty;
            var map2 = GroupMap<int, string>.Empty
                .Add(Group.From(new[] { 1 }, "one"));

            var combined = GroupMap.Combine(map1, map2);
            Assert.That(combined, Is.EqualTo(map2));
        }
        
        [Test]
        public void OneEmpty_AndOne()
        {
            var map1 = GroupMap<int, string>.Empty
                .Add(Group.From(new[] { 1 }, "one"));
            var map2 = GroupMap<int, string>.Empty;

            var combined = GroupMap.Combine(map1, map2);
            Assert.That(combined, Is.EqualTo(map1));
        }
        
    }
}