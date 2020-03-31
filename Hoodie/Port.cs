using System;

namespace Hoodie
{
    public class Port
    {
        public readonly string Name;
        private readonly Ripple _ripple;

        public Port(string name, Ripple ripple)
        {
            Name = name;
            _ripple = ripple;
        }

        public DisjunctGraph<Domain> Update(Domain domain)
            => _ripple(domain);

        public override string ToString()
            => $"Port({Name})";
    }

    public delegate DisjunctGraph<Domain> Ripple(Domain domain);
}