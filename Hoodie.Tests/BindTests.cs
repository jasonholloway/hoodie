using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;

namespace Hoodie.Tests
{
    public class Bind
    {
        private readonly Lookup<Port, (Domain, Graph)> _lookup;

        private Bind(Lookup<Port, (Domain,Graph)> lookup)
        {
            _lookup = lookup;
        }

        public Bind(params (IEnumerable<Port>, IEnumerable<(Domain, Graph)>)[] entries)
        {
            _lookup = new Lookup<Port, (Domain, Graph)>(entries);
        }

        public IEnumerable<(Domain, Graph)> this[Port port]
            => _lookup[port];

        public static readonly Bind Zero = new Bind();

        public static Bind Lift(IEnumerable<Port> ports, IEnumerable<(Domain, Graph)> domainEnvs)
            => new Bind((ports, domainEnvs));

        public static Bind Combine(Bind left, Bind right)
            => new Bind(Lookup<Port, (Domain, Graph)>.Combine(left._lookup, right._lookup));
    }
    
    public class BindTests
    {
        [Test]
        public void BindLeft()
        {
            var port = new Port("somePort", default);

            var combined = Bind.Combine(
                Bind.Lift(new[] {port}, new[] {(Domains.Bool, Graph.Empty)}),
                Bind.Zero);
            
            var disjuncts = combined[port];
            disjuncts.ShouldHaveSingleItem();
        }
        
        [Test]
        public void BindRight()
        {
            var port = new Port("somePort", default);

            var combined = Bind.Combine(
                Bind.Zero, 
                Bind.Lift(new[] {port}, new[] {(Domains.Bool, Graph.Empty)}));
            
            var disjuncts = combined[port];
            disjuncts.ShouldHaveSingleItem();
        }
    }
    
}