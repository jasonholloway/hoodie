using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;

namespace Hoodie.Tests
{
    public class Bind
    {
        private readonly Lookup<Port, (Domain, Graph)> _lookup;

        public Bind(params (IEnumerable<Port>, IEnumerable<(Domain, Graph)>)[] entries)
        {
            _lookup =new Lookup<Port, (Domain, Graph)>(entries);
        }

        public IEnumerable<(Domain, Graph)> this[Port port] => null;

        public static Bind Zero = new Bind();
        
        public static Bind Lift(Port[] ports, Domain domain, Graph graph) 
            => new Bind((ports, new[] { (domain, graph) }));

        public static Bind Combine(Bind left, Bind right)
            => left;
    }
    
    public class BindTests
    {
        [Test]
        public void BindSomethingOrOther()
        {
            var port = new Port("somePort", default);

            var bind1 = new Bind((new[] { port }, new[] { (Domains.Bool, Graph.Empty) }));
            var bind2 = new Bind((new[] { port }, new[] { (Domains.Bool, Graph.Empty) }));

            var combined = Bind.Combine(bind1, bind2);
            
            var disjuncts = combined[port];
            disjuncts.ShouldHaveSingleItem();
        }
    }
    
}