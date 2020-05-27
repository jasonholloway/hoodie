using NUnit.Framework;
using static Hoodie.GroupMaps.Tests.MapLang.Runner;

namespace Hoodie.GroupMaps.Tests
{
    public class ClumpTests
    {
        [Test]
        public void MapEquality1()
            => Test(@"
                    A | A
                    A = A
                ");

        [Test]
        public void MapEquality2()
            => Test(@"
                    A . | . A
                    A B = B A
                ");

        [Test]
        public void MapEquality3()
            => Test(@"
                    A . | . A
                    A B = B A
                    C . | C .
                ");
    }

    public class HitTests
    {
        [Test]
        public void Simple()
            => Test(@"
                A => A
            ");
        
        [Test]
        public void Simple_WithMask1()
            => Test(@"
                A |  .
                B => B
            ");

        [Test]
        public void Simple_Disjunct()
            => Test(@"
                A B => A ^ B
            ");
        
        
        
        [Test]
        public void Complicated()
            => Test(@"
                A . C  |   _ _ _ _ _
                . B .  =>  . ^ . ^ B
                A B .  =>  A | . | B
            ");
    }
}