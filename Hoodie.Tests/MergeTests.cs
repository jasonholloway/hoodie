using NUnit.Framework;
using Shouldly;
using static Hoodie.GraphOps;

namespace Hoodie.Tests
{
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

            var env3 = Env.Merge(env1, env2);

            var binds = env3.GetBinds(@var.Port);

            binds.ShouldHaveSingleItem();
        }


        Env BuildEnv<T>(Graph<T> graph)
            => graph(Env.Empty).Item1;
    }
}