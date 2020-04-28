using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace Hoodie.Tests
{
    public readonly struct Disjunction
    {
        public readonly ImmutableArray<(Domain, Bind)> DomainEnvs;

        public Disjunction(IEnumerable<(Domain, Bind)> domainEnvs)
        {
            DomainEnvs = domainEnvs.ToImmutableArray();
        }
    }
    
    
    public class Bind
    {
        private readonly GroupMap<Port, Disjunction> _groups;

        private Bind(GroupMap<Port, Disjunction> groups)
        {
            _groups = groups;
        }

        public Bind(params (IEnumerable<Port>, IEnumerable<(Domain, Bind)>)[] entries)
        {
            _groups = entries.Aggregate(
                GroupMap<Port, Disjunction>.Empty,
                (map, entry) => map.Add(Group.From(entry.Item1, new Disjunction(entry.Item2))));
        }

        public IEnumerable<Group<Port, Disjunction>> this[Port port]
            => _groups[port];

        public static readonly Bind Zero = new Bind();

        public static Bind Lift(IEnumerable<Port> ports, IEnumerable<(Domain, Bind)> domainEnvs)
            => new Bind((ports, domainEnvs));

        public static Bind Combine(Bind left, Bind right)
            => new Bind(GroupMap.Combine(left._groups, right._groups));
    }
    
    public class BindTests
    {
        [Test]
        public void BindOneLeft()
        {
            var port = new Port("somePort", default);

            var combined = Bind.Combine(
                Bind.Lift(new[] {port}, new[] {(Domains.Bool, Bind.Zero)}),
                Bind.Zero);
            
            var portsets = combined[port];
            portsets.ShouldHaveSingleItem();
            portsets.First().Value.DomainEnvs.Single().Item1.ShouldBe(Domains.Bool);
            portsets.First().Value.DomainEnvs.Single().Item2.ShouldBe(Bind.Zero);
        }
        
        [Test]
        public void BindOneRight()
        {
            var port = new Port("somePort", default);

            var combined = Bind.Combine(
                Bind.Zero, 
                Bind.Lift(new[] {port}, new[] {(Domains.Bool, Bind.Zero)}));
            
            var disjuncts = combined[port];
            disjuncts.ShouldHaveSingleItem();
            disjuncts.First().Value.DomainEnvs.Single().Item1.ShouldBe(Domains.Bool);
            disjuncts.First().Value.DomainEnvs.Single().Item2.ShouldBe(Bind.Zero);
        }
        
        [Test]
        public void BindBoth()
        {
            var port = new Port("somePort", default);

            var combined = Bind.Combine(
                Bind.Lift(new[] {port}, new[] {(Domains.True, Bind.Zero)}),
                Bind.Lift(new[] {port}, new[] {(Domains.False, Bind.Zero)}));
            
            var portsets = combined[port];
            portsets.Count().ShouldBe(1);
            portsets.Single().Nodes.Single().ShouldBe(port);
            
            var disjuncts = portsets.Single().Value.DomainEnvs;
            disjuncts.ElementAt(0).Item1.ShouldBe(Domains.True);
            disjuncts.ElementAt(0).Item2.ShouldBe(Bind.Zero);
            disjuncts.ElementAt(1).Item1.ShouldBe(Domains.False);
            disjuncts.ElementAt(1).Item2.ShouldBe(Bind.Zero);
        }
        
        [Test]
        public void BindWithDomainMerge()
        {
            var port = new Port("somePort", default);

            var combined = Bind.Combine(
                Bind.Lift(new[] {port}, new[] {(Domains.True, Bind.Zero)}),
                Bind.Lift(new[] {port}, new[] {(Domains.False, Bind.Zero)}));
            
            //but there are two kinds of combination
            //one, where domains are actually merged down
            //and another where they're suspended in disjunction
            //...
            //hmmmmmm
            
            //each entry in each disjunction combines which each other
            //the disjunctions then cancel themselves down
            //
            //THE GRAPHS ARE THEN COMBINED?
            
            var portsets = combined[port];
            portsets.Count().ShouldBe(1);
            portsets.Single().Nodes.Single().ShouldBe(port);
            
            var disjuncts = portsets.Single().Value.DomainEnvs;
            disjuncts.ElementAt(0).Item1.ShouldBe(Domains.True);
            disjuncts.ElementAt(0).Item2.ShouldBe(Bind.Zero);
            disjuncts.ElementAt(1).Item1.ShouldBe(Domains.False);
            disjuncts.ElementAt(1).Item2.ShouldBe(Bind.Zero);
        }
    }
    
}