using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace Hoodie
{
    public class Binding : IEquatable<Binding>
    {
        public readonly ImmutableHashSet<Port> Ports;
        public readonly ImmutableArray<(Domain, Graph)> DomainEnvs;

        public Binding(ImmutableHashSet<Port> ports, ImmutableArray<(Domain, Graph)> domainEnvs)
        {
            Ports = ports;
            DomainEnvs = domainEnvs;
        }
        
        public Binding(Domain domain, Graph graph)
            : this(ImmutableHashSet<Port>.Empty, ImmutableArray<(Domain, Graph)>.Empty.Add((domain, graph)))
        { }

        public Binding()
            : this(ImmutableHashSet<Port>.Empty, ImmutableArray<(Domain, Graph)>.Empty)
        { }

        public Binding WithDomain(Domain domain)
        {
            throw new NotImplementedException();
        }

        public Binding AddPort(Port port)
        {
            throw new NotImplementedException();
        }

        public static readonly Binding Empty = new Binding();

        public static Binding Merge(Binding b1, Binding b2)
            => Empty;

        public bool Equals(Binding other)
        {
            //TODO should have unique ids, as with domains - though, couldn't we just do a reference comparison???
            throw new NotImplementedException();
        }
    }
}