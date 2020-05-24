using System.Collections.Generic;
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

        [Test]
        public void ReadDisjunction1()
        {
            var result = Run<ISet<Map<int, Sym>>>(@"
                            A ^ B
                        ");

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void ReadDisjunction2()
        {
            var result = Run<ISet<Map<int, Sym>>>(@"
                            A ^ B ^ C
                        ");

            Assert.That(result.Count, Is.EqualTo(3));
        }
    }
}