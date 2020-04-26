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

        public DisjunctOp<Domain> Propagate(Domain domain)
            => _ripple(domain);

        public override string ToString()
            => $"Port({Name})";
    }

    public delegate DisjunctOp<Domain> Ripple(Domain domain);
    
    //a binding comes in then...
    //yes, it's true, we return a disjuntion here
    //
    //the difference is that we expect it as part of a 'disjunctgraph'
    //
    //this allows us to propagate away, only caring about our current domain...
    //
    //
    //
    
    
    
}