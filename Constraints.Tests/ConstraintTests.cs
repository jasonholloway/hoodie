using NUnit.Framework;
using probs;
using Shouldly;

namespace Constraints.Tests
{
    using static Ops;
    
    public class ConstraintTests
    {
        [Test]
        public void SimpleEquality()
        {
            var country = new Variable();
            Assert(country == "AU");
            
            country.Sample().ShouldBe("AU");
        }
    }
}