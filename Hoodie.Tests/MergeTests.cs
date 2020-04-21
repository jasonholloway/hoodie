using System.Linq;
using NUnit.Framework;
using Shouldly;
using static Hoodie.GraphOps;

namespace Hoodie.Tests
{
    public class EnvMergeTests
    {
        [Test]
        public void Woooo()
        {
            
        }
        
    }





    public class MergeTests
    {
        [Test]
        public void Blah()
        {
            var @var = new Var("test");

            var env1 = BuildEnv(
                from k in Bind(@var, new TrueDomain())
                select 1);
            
            var env2 = BuildEnv( 
                from k in Bind(@var, Domains.Any)
                select 1);

            var env3 = Graph.Merge(env1, env2);

            var binds = env3.GetBinds(@var.Port);

            binds.ShouldHaveSingleItem();
            var bind = binds.First();
            
            bind.Ports.ShouldHaveSingleItem();
            bind.Ports.Single().ShouldBe(@var.Port);

            bind.DomainEnvs.ShouldHaveSingleItem();
            var (domain, env) = bind.DomainEnvs.First();
            domain.ShouldBeOfType<TrueDomain>();
        }

        Graph BuildEnv<T>(GraphOp<T> graphOp)
        {
            var (env, _) = graphOp(Graph.Empty);
            return env;
        }
    }
}