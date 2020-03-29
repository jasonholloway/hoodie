using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Hoodie
{
    public class Binding
    {
        private readonly ImmutableHashSet<Port> _ports;
        public readonly Domain Domain;

        public Binding(ImmutableHashSet<Port> ports, Domain domain)
        {
            _ports = ports;
            Domain = domain;
        }

        public Binding() 
            : this(ImmutableHashSet<Port>.Empty, Domains.Any)
        { }

        public IEnumerable<Port> Ports => _ports;

        public Binding SetDomain(Domain domain)
        {
            throw new NotImplementedException();
        }

        public Binding AddPort(Port port)
        {
            throw new NotImplementedException();
        }
    }
}