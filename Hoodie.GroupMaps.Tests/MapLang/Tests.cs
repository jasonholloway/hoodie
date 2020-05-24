using System.Collections.Generic;
using NUnit.Framework;
using static Hoodie.GroupMaps.Tests.MapLang.Runner;

namespace Hoodie.GroupMaps.Tests.MapLang
{
    public class MapLangTests
    {
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
        
        [Test]
        public void ReadDisjunction3()
        {
            var result = Run<ISet<Map<int, Sym>>>(@"
                            A ^ . ^ .
                            . ^ . ^ C
                        ");

            Assert.That(result.Count, Is.EqualTo(3));
        }
        
    }
}