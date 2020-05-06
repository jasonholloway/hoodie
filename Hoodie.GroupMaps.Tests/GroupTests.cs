using System.Collections.Generic;
using NUnit.Framework;

namespace Hoodie.GroupMaps.Tests
{
    public class GroupTests
    {
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
    }
}