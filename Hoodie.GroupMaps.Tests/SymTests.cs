using System.Linq;
using NUnit.Framework;

namespace Hoodie.GroupMaps.Tests
{
    public class SymTests
    {
        [Test]
        public void Equality()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Sym.From('A'), Is.EqualTo(Sym.From('A')));
                Assert.That(Sym.From("HAMSTER"), Is.EqualTo(Sym.From("HAMSTER")));
            });
        }

        [Test]
        public void HashCodes()
        {
            var syms = new Sym[]
            {
                'A', 'A', 'A'
            };

            var hashes = syms.Select(s => s.GetHashCode()).ToHashSet();
            Assert.That(hashes, Has.Count.EqualTo(1));
        }
    }
}