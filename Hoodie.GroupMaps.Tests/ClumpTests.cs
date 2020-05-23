using NUnit.Framework;
using static Hoodie.GroupMaps.Tests.MapLang;

namespace Hoodie.GroupMaps.Tests
{
    public class ClumpTests
    {
        [Test]
        public void MapEquality1()
            => Run(@"
                    A | A
                    A = A
                ");

        [Test]
        public void MapEquality2()
            => Run(@"
                    A . | . A
                    A B = B A
                ");

        [Test]
        public void MapEquality3()
            => Run(@"
                    A . | . A
                    A B = B A
                    C . | C .
                ");

        [Test]
        public void Blah()
            => Run(@"
                    A . C  |   _ _ _ _ _
                    . B .  =>  . ^ . ^ B
                    A B .  =>  A | . | B
                ");

    }
}