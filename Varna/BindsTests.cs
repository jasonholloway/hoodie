using NUnit.Framework;

namespace Varna
{
    internal class BindsTests
    {
        [Test]
        public void Combine_Happy()
        {
            var d1 = Binds.Empty
                .Set("x", 2);

            var d2 = Binds.Empty
                .Set("y", 4);

            var d3 = d1.Combine(d2);

            var gotX = d3.Get("x");
            Assert.That(gotX, Is.TypeOf<Int>());
            Assert.That(((Int) gotX).Value, Is.EqualTo(2));
            
            var gotY = d3.Get("y");
            Assert.That(gotY, Is.TypeOf<Int>());
            Assert.That(((Int) gotY).Value, Is.EqualTo(4));
        }
        
        [Test]
        public void Combine_NullOnClash()
        {
            var d1 = Binds.Empty
                .Set("x", 2)
                .Set("y", 3);

            var d2 = Binds.Empty
                .Set("x", 2)
                .Set("y", 4);

            var d3 = d1.Combine(d2);
            
            Assert.That(d3, Is.EqualTo(null));
        }
        
        [Test]
        public void BindsEqualityTests()
        {
            var d1 = Binds.Empty
                .Set("x", 2)
                .Set("y", 3);
                
            var d2 = Binds.Empty
                .Set("y", 3)
                .Set("x", 2);

            Assert.That(d1.Equals(d2), Is.True);
        }
            
        [Test]
        public void BindsInequalityTests()
        {
            var d1 = Binds.Empty
                .Set("x", 2)
                .Set("y", 4);
                
            var d2 = Binds.Empty
                .Set("y", 3)
                .Set("x", 2);

            Assert.That(d1.Equals(d2), Is.False);
        }
                
    }
}