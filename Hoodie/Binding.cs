using System;
using System.Collections.Immutable;

namespace Hoodie
{
    public class Binding : IEquatable<Binding>
    {
        public readonly ImmutableHashSet<Port> Ports;
        public readonly Domain Domain;

        public Binding(ImmutableHashSet<Port> ports, Domain domain)
        {
            Ports = ports;
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