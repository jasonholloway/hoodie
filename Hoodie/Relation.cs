namespace Hoodie
{
    using static GraphOps;
    using static DomainOps;
    
    public abstract class Relation
    {
    }
    
    public class Var : Relation
    {
        public readonly Port Port;

        public Var(string name)
        {
            Port = new Port($"Var({name})",
                @in => Graph.Lift(Domains.Any));
        }
    }

    public class AreEqualConstraint : Relation
    {
        public readonly Port Left;
        public readonly Port Right;
        public readonly Port Result;

        public AreEqualConstraint()
        {
            Left = new Port(nameof(Left), 
                @in =>
                    from right in Domain(Right)
                    from result in Domain(Result)
                    from d2 in (right, result) switch 
                    {
                        (_, TrueDomain _) => Bind(Right, @in),
                        (_, FalseDomain _) => Bind(Right, Invert(@in)),
                        (_, BoolDomain _) => Bind(Result, @in),     //if result isn't set, then 
                        _ => Zap(Right, Result)
                    }
                    select d2);

            Right = new Port(nameof(Right),
                @in => //TODO etc etc
                    from right in Domain(Right)
                    from result in Domain(Result)
                    from d2 in (right, result) switch
                    {
                        (_, TrueDomain _) => Bind(Left, @in),
                        (_, FalseDomain _) => Bind(Left, @in),
                        (_, BoolDomain _) => Bind(Result, @in),
                        _ => Zap(Left, Result)
                    }
                    select d2);

            Result = new Port(nameof(Result),
                @in => //TODO etc etc
                    from right in Domain(Right)
                    from result in Domain(Result)
                    from d2 in (right, result) switch
                    {
                        (_, TrueDomain _) => Bind(Right, @in),
                        (_, FalseDomain _) => Bind(Right, @in),
                        (_, BoolDomain _) => Bind(Result, @in),
                        _ => Zap(Right, Result)
                    }
                    select d2);
        }
    }

    public class GreaterThanConstraint : Relation
    {
        public readonly Port Left;
        public readonly Port Right;
        public readonly Port Result;

        public GreaterThanConstraint()
        {
            Left = new Port(nameof(Left),
                @in => Graph.Lift(Domains.Any));

            Right = new Port(nameof(Right),
                @in => Graph.Lift(Domains.Any));
            
            Result = new Port(nameof(Result), 
                @in => Graph.Lift(Domains.Any));
        }
    }

    public class IsNumberConstraint : Relation
    {
        public readonly Port Inner;
        public readonly Port Result;

        public IsNumberConstraint()
        {
            Inner = new Port(nameof(Inner),
                @in => Graph.Lift(Domains.Any));
            
            Result = new Port(nameof(Result),
                @in => Graph.Lift(Domains.Any));
        }
    }
}