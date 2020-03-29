using System.Linq;

namespace Hoodie
{
    public static class GraphOps
    {
        public static Graph<Var> Var(string name)
            => env => env.SummonVar(name);

        public static Graph<Domain> Bind(params Bindable[] bindables)
            => BaseOps.Bind(bindables.AsEnumerable());
        
        public static Graph<Port> AreEqual(Bindable left, Bindable right)
        {
            var areEqual = new AreEqualConstraint();
            return
                from d1 in Bind(left, areEqual.Left)
                from d2 in Bind(right, areEqual.Right)
                select areEqual.Result;
        }
        
        public static Graph<Port> GreaterThan(Bindable left, Bindable right)
        {
            var greaterThan = new GreaterThanConstraint();
            return
                from d1 in Bind(left, greaterThan.Left)
                from d2 in Bind(right, greaterThan.Right)
                select greaterThan.Result;
        }
        
        public static Graph<Port> IsNumber(Bindable sub)
        {
            var isNumber = new IsNumberConstraint();
            return
                from d in Bind(sub, isNumber.Inner)
                select isNumber.Result;
        }

        public static Graph<Domain> Assert(Port port)
            => Bind(port, true);

        public static Graph<Domain> Assert(Graph<Port> graph)
            => from port in graph
                from domain in Assert(port)
                select domain;

        public static DisjunctGraph<Domain> Domain(Port port)
            => new DisjunctGraph<Domain>(env => env.GetDomain(port));

        public static Graph<Domain> Zap(params Port[] ports)
            => BaseOps.Bind(Enumerable
                .Repeat((Bindable)Domains.Never, 1)
                .Concat(ports.Select(p => (Bindable)p)));
    }
}