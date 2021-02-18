namespace Varna
{
    class Reader
    {
        public static Scope Read(Scope scope)
            => Read(scope, (dynamic)scope.Exp);

        private static Scope Read(Scope s, LeafExp _)
            => s;
        
        private static Scope Read(Scope s, OrExp x)
            => (x.Left.Exp, x.Right.Exp) switch
            {
                (Never _, Exp _) => new Scope(),
                (Exp _, Never _) => new Scope(),
                _ => new Scope(
                    new OrExp(
                        Read(x.Left.Exp), 
                        Read(x.Right.Exp)), 
                    s.Binds)
            };

        private static Scope Read(Scope s, EqualsExp x)
        {
            switch (x.Left.Exp, x.Right.Exp)
            {
                case (Var var, Exp exp):
                    var s2 = Read(exp);

                    if (s2.Exp is OrExp or)
                    {
                        //if it is an or, then we can distribute the assignment across the two legs
                        return new Scope(
                            new OrExp(
                                Read(new EqualsExp(var, or.Left)), 
                                Read(new EqualsExp(var, or.Right))),
                            s.Binds //!!!!
                        );
                    }
                    else
                    {
                        return new Scope(
                            new True(),
                            x.Left.Binds
                                .AddRange(x.Right.Binds)
                                .Add(var.Name, s2.Exp)
                        );
                    }
            }
            
            return new Scope();
        }

        private static Scope Read(Scope s, AndExp x)
        {
            var left = Read(x.Left.Exp);
            var right = Read(x.Right.Exp);

            return (left.Exp, right.Exp) switch
            {
                (Int l, Int r) =>
                    (l.Value == r.Value)
                        ? new Scope(l, left.Binds.SetItems(right.Binds))
                        : new Scope(),

                (True _, True _) =>
                    new Scope(
                        new True(),
                        left.Binds.SetItems(right.Binds)),

                _ => new Scope()
            };
        }
    }
}