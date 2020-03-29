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
        
        public Binding(Domain domain)
            : this(ImmutableHashSet<Port>.Empty, domain)
        { }

        public Binding(Port port)
            : this(ImmutableHashSet<Port>.Empty.Add(port), Domains.Any)
        { }

        public Binding() 
            : this(ImmutableHashSet<Port>.Empty, Domains.Any)
        { }

        public IEnumerable<Port> Ports => _ports;

        public Binding AddDomain(Domain domain)
        {
            throw new NotImplementedException();
        }

        public Binding AddPort(Port port)
        {
            throw new NotImplementedException();
        }
        
        public static Binding Empty = new Binding(ImmutableHashSet<Port>.Empty, Domains.Any);

        public static Binding Merge(Binding b1, Binding b2)
            => Binding.Empty;
    }
}