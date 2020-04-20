using System;
using System.Collections.Immutable;

namespace Hoodie
{
    public class Binding : IEquatable<Binding>
    {
        public readonly ImmutableHashSet<Port> Ports;
        public readonly ImmutableArray<(Domain, Env)> DomainEnvs;

        public Binding(ImmutableHashSet<Port> ports, ImmutableArray<(Domain, Env)> domainEnvs)
        {
            Ports = ports;
            DomainEnvs = domainEnvs;
        }
        
        public Binding(Domain domain, Env env)
            : this(ImmutableHashSet<Port>.Empty, ImmutableArray<(Domain, Env)>.Empty.Add((domain, env)))
        { }

        public Binding()
            : this(ImmutableHashSet<Port>.Empty, ImmutableArray<(Domain, Env)>.Empty)
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