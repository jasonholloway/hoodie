using NUnit.Framework;
using Shouldly;

namespace Hoodie.Tests
{
    using static GraphOps;
    
    public class Tests
    {
        [Test]
        public void SuperSimpleSystem()
        {
            var graph =
                from varX in Var("x")
                from _ in Assert(AreEqual(varX, 3))
                select varX;

            var (env, x) = graph(new Env());
            
            var domain = env.GetDomain(x.Port);
            domain.ShouldNotBeNull();
        }
    }
}