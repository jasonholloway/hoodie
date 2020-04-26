using System.Linq;

namespace Hoodie
{
    
    public struct Nil {}

    public static class Extensions
    {
        public static GraphOp<Nil> Effect<T>(this GraphOp<T> op)
            => op.Select(_ => default(Nil));
        
        public static GraphOp<Nil> Effect<T>(this DisjunctOp<T> op)
            => op.Invoke.Effect();
    }
    
    public static class GraphOps
    {
        public static GraphOp<Var> Var(string name)
            => env => env.SummonVar(name);

        //and so bind creates a fresh env fragment
        //which then gets merged in to the whole
        //but the fundamental operation here is the merge
        //so binding is really about forming a small env and merging it in
        
        //
        //so firtly we need to create atoms of envs: what does the smallest graph have?
        //simple - a binding; and to have a binding, it must have a port;
        //we could have a graph with a port with an empty binding
        //
        //but every graph has bindings for every possible port - this is the graph's, and the ports', original state 
        //the graph by itself says for every port: you are unconstrained! 
        //the first, smallest atom of information is a narrowing of the smallest possible thing, which would be one port
        //
        //so, smallest graph: a single port domain, like a splinter
        //what is this smallest unit? an atom! no, something negative - peccadillo, flaw, sin, narrowing, atom again
        //a binding - which is already a negative term; a Bind
        //
        //Binds are as always monoidal
        //are they commutative too? I think so, yes: they should always sit in their proper order
        //
        //
        
        public static DisjunctOp<Domain> Bind(params Bindable[] bindables)
            => BaseOps.Bind(bindables.AsEnumerable());
        
        public static GraphOp<Port> AreEqual(Bindable left, Bindable right)
        {
            var areEqual = new AreEqualConstraint();
            return
                from _1 in Bind(left, areEqual.Left).Effect()
                from _2 in Bind(right, areEqual.Right).Effect()
                select areEqual.Result;
        }
        
        public static GraphOp<Port> GreaterThan(Bindable left, Bindable right)
        {
            var greaterThan = new GreaterThanConstraint();
            return
                from _1 in Bind(left, greaterThan.Left).Effect()
                from _2 in Bind(right, greaterThan.Right).Effect()
                select greaterThan.Result;
        }
        
        public static GraphOp<Port> IsNumber(Bindable sub)
        {
            var isNumber = new IsNumberConstraint();
            return
                from _ in Bind(sub, isNumber.Inner).Effect()
                select isNumber.Result;
        }

        public static GraphOp<Nil> Assert(Port port)
            => Bind(port, true).Effect();

        public static GraphOp<Nil> Assert(GraphOp<Port> graphOp)
            => from port in graphOp
               from _ in Assert(port)
               select _;

        public static DisjunctOp<Domain> Domain(Port port)
            => new DisjunctOp<Domain>(env => env.GetDomain(port));

        public static DisjunctOp<Domain> Zap(params Port[] ports)
            => BaseOps.Bind(Enumerable
                .Repeat((Bindable)Domains.Never, 1)
                .Concat(ports.Select(p => (Bindable)p)));
    }
}