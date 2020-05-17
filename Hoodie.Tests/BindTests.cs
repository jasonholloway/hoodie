using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Hoodie.GroupMaps;
using NUnit.Framework;
using Shouldly;

namespace Hoodie.Tests
{
    public readonly struct DomainSet
    {
        public readonly ImmutableArray<(Domain, Bind)> DomainEnvs;

        public DomainSet(IEnumerable<(Domain, Bind)> domainEnvs)
        {
            DomainEnvs = domainEnvs.ToImmutableArray();
        }
    }

    public class PortSet
    {
        private readonly Group<Port, DomainSet> _group;

        public PortSet(Group<Port, DomainSet> group)
        {
            _group = group;
        }

        public IEnumerable<Port> Ports => _group.Nodes;
        public IEnumerable<(Domain, Bind)> Domains => _group.Value.DomainEnvs;
    }
    
    public class Bind
    {
        private readonly Map<Port, DomainSet> _map;

        private Bind(Map<Port, DomainSet> map)
        {
            _map = map;
        }

        public Bind(params (IEnumerable<Port>, IEnumerable<(Domain, Bind)>)[] entries)
        {
            _map = entries.Aggregate(
                Map<Port, DomainSet>.Empty,
                (map, entry) => map.Add(GroupMap.Lift(entry.Item1, new DomainSet(entry.Item2))));
        }

        public IEnumerable<PortSet> this[Port port]
            => _map[port].Select(g => new PortSet(g));

        public static readonly Bind Zero = new Bind();

        public static Bind Lift(IEnumerable<Port> ports, IEnumerable<(Domain, Bind)> domainEnvs)
            => new Bind((ports, domainEnvs));

        public static Bind Combine(Bind left, Bind right)
            => new Bind(left._map.Combine(right._map, null));
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
            portsets.First().Domains.Single().Item1.ShouldBe(Domains.Bool);
            portsets.First().Domains.Single().Item2.ShouldBe(Bind.Zero);
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
            disjuncts.First().Domains.Single().Item1.ShouldBe(Domains.Bool);
            disjuncts.First().Domains.Single().Item2.ShouldBe(Bind.Zero);
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
            portsets.Single().Ports.Single().ShouldBe(port);

            var domains = portsets.Single().Domains;
            domains.ElementAt(0).Item1.ShouldBe(Domains.True);
            domains.ElementAt(0).Item2.ShouldBe(Bind.Zero);
            domains.ElementAt(1).Item1.ShouldBe(Domains.False);
            domains.ElementAt(1).Item2.ShouldBe(Bind.Zero);
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
            portsets.Single().Ports.Single().ShouldBe(port);

            var domains = portsets.Single().Domains;
            domains.ElementAt(0).Item1.ShouldBe(Domains.True);
            domains.ElementAt(0).Item2.ShouldBe(Bind.Zero);
            domains.ElementAt(1).Item1.ShouldBe(Domains.False);
            domains.ElementAt(1).Item2.ShouldBe(Bind.Zero);
        }
    }
    
}