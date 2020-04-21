using System.Linq;

namespace Hoodie
{
    public static class GraphOps
    {
        public static GraphOp<Var> Var(string name)
            => env => env.SummonVar(name);

        //and so bind creates a fresh env fragment
        //which then gets merged in to the whole
        //but the fundamental operation here is the merge
        //so binding is really about forming a small env and merging it in
        
        //but not only 
        //
        //
        //
        
        public static GraphOp<Domain> Bind(params Bindable[] bindables)
            => BaseOps.Bind(bindables.AsEnumerable());
        
        public static GraphOp<Port> AreEqual(Bindable left, Bindable right)
        {
            var areEqual = new AreEqualConstraint();
            return
                from d1 in Bind(left, areEqual.Left)
                from d2 in Bind(right, areEqual.Right)
                select areEqual.Result;
        }
        
        public static GraphOp<Port> GreaterThan(Bindable left, Bindable right)
        {
            var greaterThan = new GreaterThanConstraint();
            return
                from d1 in Bind(left, greaterThan.Left)
                from d2 in Bind(right, greaterThan.Right)
                select greaterThan.Result;
        }
        
        public static GraphOp<Port> IsNumber(Bindable sub)
        {
            var isNumber = new IsNumberConstraint();
            return
                from d in Bind(sub, isNumber.Inner)
                select isNumber.Result;
        }

        public static GraphOp<Domain> Assert(Port port)
            => Bind(port, true);

        public static GraphOp<Domain> Assert(GraphOp<Port> graphOp)
            => from port in graphOp
                from domain in Assert(port)
                select domain;

        public static DisjunctGraph<Domain> Domain(Port port)
            => new DisjunctGraph<Domain>(env => env.GetDomain(port));

        public static GraphOp<Domain> Zap(params Port[] ports)
            => BaseOps.Bind(Enumerable
                .Repeat((Bindable)Domains.Never, 1)
                .Concat(ports.Select(p => (Bindable)p)));
    }
}