using System.Collections.Immutable;

namespace Hoodie
{
    public class Disjunction
    {
        private ImmutableDictionary<PortSet, Domain[]> _portsetDomains;

        public Disjunction(ImmutableDictionary<PortSet, Domain[]> portsetDomains)
        {
            _portsetDomains = portsetDomains;
        }
    }
}