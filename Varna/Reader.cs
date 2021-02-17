namespace Varna
{
    class Reader
    {
        public static Scope Read(Scope scope)
            => Visit(scope, (dynamic)scope.Exp);

        private static Scope Visit(Scope s, LeafExp _)
            => s;
        
        private static Scope Visit(Scope s, OrExp x)
        {
            switch (x.Left.Exp, x.Right.Exp)
            {
                case (Var var, Exp exp):
                    var x2 = Read(exp);
                    
                    return new Scope(
                        new True(),
                        x.Left.Binds
                            .AddRange(x.Right.Binds)
                            .Add(var.Name, x2.Exp)
                    );
            }
            
            return new Scope();
        }

        private static Scope Visit(Scope s, AndExp x)
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

        private static Scope Visit(Scope s, EqualsExp x)
        {
            switch (x.Left.Exp, x.Right.Exp)
            {
                case (Var var, Exp exp):
                    var x2 = Read(exp);
                    
                    return new Scope(
                        new True(),
                        x.Left.Binds
                            .AddRange(x.Right.Binds)
                            .Add(var.Name, x2.Exp)
                    );
            }
            
            return new Scope();
        }
    }
}