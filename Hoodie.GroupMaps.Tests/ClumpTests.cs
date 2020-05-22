using System.Collections.Immutable;
using NUnit.Framework;

namespace Hoodie.GroupMaps.Tests
{

    public class Clumpable
    {
        public ImmutableHashSet<Clumpable> Disjuncts { get; }
    }
    
    public class ClumpTests
    {
        [Test]
        public void Blah()
            => Test(@"
                A . D .
                A B . .
                . B C % 
                A^D, A^B, B^C
                ");
        
        //the above should give us a clump


        static void Test(string blah)
        {
            
        }
        
    }
}