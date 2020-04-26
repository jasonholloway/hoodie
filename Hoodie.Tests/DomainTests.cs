using NUnit.Framework;
using Shouldly;

namespace Hoodie.Tests
{
    public class DomainTests
    {
        [Test]
        public void TrueFalse_Never()
        {
            var result = Domain.Combine(new TrueDomain(), new FalseDomain());
            result.ShouldBeOfType<NeverDomain>();
        }
        
        [Test]
        public void TrueTrue_True()
        {
            var result = Domain.Combine(new TrueDomain(), new TrueDomain());
            result.ShouldBeOfType<TrueDomain>();
        }
        
        [Test]
        public void FalseFalse_False()
        {
            var result = Domain.Combine(new FalseDomain(), new FalseDomain());
            result.ShouldBeOfType<FalseDomain>();
        }
        
        [Test]
        public void TrueBool_True()
        {
            var result = Domain.Combine(new TrueDomain(), new BoolDomain());
            result.ShouldBeOfType<TrueDomain>();
        }
        
        [Test]
        public void FalseBool_False()
        {
            var result = Domain.Combine(new FalseDomain(), new BoolDomain());
            result.ShouldBeOfType<FalseDomain>();
        }
        
        [Test]
        public void TrueAny_True()
        {
            var result = Domain.Combine(new TrueDomain(), new AnyDomain());
            result.ShouldBeOfType<TrueDomain>();
        }
    }
}