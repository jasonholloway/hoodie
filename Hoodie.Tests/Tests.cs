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

            var (env, x) = graph(Graph.Empty);
            
            var domain = env.GetDomain(x.Port);
            domain.ShouldNotBeNull();
        }
        
        [Test]
        public void Sandbox()
        {
            var graph =
                from v in Var("v")
                from _1 in Assert(AreEqual(v, 13))
                from _2 in Assert(AreEqual(v, 17))
                select v;

            var (env, x) = graph(Graph.Empty);

            var domain = env.GetDomain(x.Port);

        }
        
        
    }
}