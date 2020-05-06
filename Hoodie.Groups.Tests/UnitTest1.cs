using System;
using System.Collections.Immutable;
using NUnit.Framework;

namespace Hoodie.Groups.Tests
{
    public abstract class GroupGraph
    {
        public static GroupGraph<K, V> From<K, V>(K k, V v) 
            => new GroupGraph<K, V>();
    }

    public class GroupGraph<K, V> : GroupGraph
    {
        readonly ImmutableDictionary<K, V> _map = ImmutableDictionary<K, V>.Empty;

        internal GroupGraph()
        {
            
        }
        
    }
    
    
    public class Tests
    {
        [Test]
        public void Test1()
        {
            var p1 = new object();
            var p2 = new object();

            var g1 = GroupGraph.From(p1, 1);
            var g2 = GroupGraph.From(p1, 1);
            
            Assert.Pass();
        }
    }
}