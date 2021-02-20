using System.Collections.Immutable;
using System.Linq;

namespace Varna
{
    using static Ops;
    using static BindOps;

    class Reader
    {
        public static Scope Read(Scope scope)
            => Read(scope, (dynamic)scope.Exp);

        private static Scope Read(Scope s, LeafExp _)
            => s;
        
        private static Scope Read(Scope s, Var _)
            => s;
        
        private static Scope Read(Scope _, OrExp x)
        {
            var scopes2 = x.Scopes
                .Select(Read)
                .SelectMany(s => s.Exp switch
                {
                    OrExp y => y.Scopes.AsEnumerable(),
                    _ => new[] { s }
                })
                .Where(s => s.Exp is not Never)
                .ToArray();

            return scopes2.Length switch
            {
                0 => Never(),
                1 => scopes2.Single(),
                _ => new Scope(new OrExp(scopes2), InCommon(scopes2.Select(s => s.Binds)))
            };
        }

        // private static Scope DedupeOrs(Scope left, Scope right)
        // {
        //     if (right.Exp is OrExp innerRight)
        //     {
        //         if (ScopeComparer.Scope.Equals(left, innerRight.Left))
        //         {
        //             return Repack(left, innerRight.Right);
        //         }
        //         else if(ScopeComparer.Scope.Equals(left, innerRight.Right))
        //         {
        //             return Repack(left, innerRight.Left);
        //         }
        //     }
        //             
        //     if (left.Exp is OrExp innerLeft)
        //     {
        //         if (ScopeComparer.Scope.Equals(right, innerLeft.Left))
        //         {
        //             return Repack(innerLeft.Right, right);
        //         }
        //         else if(ScopeComparer.Scope.Equals(right, innerLeft.Right))
        //         {
        //             return Repack(innerLeft.Left, right);
        //         }
        //     }
        //
        //     return Repack(left, right);
        //     
        //     Scope Repack(Scope l, Scope r)
        //         => new(new OrExp(l, r), InCommon(l.Binds, r.Binds));
        // }
        
        // private static Scope DedupeOrs(Scope s)
        // {
        //     if (s.Exp is OrExp x)
        //     {
        //         return DedupeOrs(x.Left, x.Right);
        //     }
        //
        //     return s;
        // }

        private static Scope Read(Scope scope, EqualsExp x)
        {
            var left2 = Read(x.Left.Exp);
            var right2 = Read(x.Right.Exp);
            
            switch (left2.Exp, right2.Exp)
            {
                case (Var var, Exp exp):
                    if (exp is OrExp or)
                    {
                        return new Scope(
                            new OrExp(or.Scopes.Select(s => Read(new EqualsExp(var, s)))),
                            scope.Binds //!!!!
                        );
                    }
                    else
                    {
                        return new Scope(
                            new True(),
                            x.Left.Binds
                                .Combine(x.Right.Binds)?
                                .Set(var.Name, exp)
                        );
                    }
                
                default: return Never();
            }
        }

        private static Scope Read(Scope s, AndExp x)
        {
            var left = Read(x.Left.Exp);
            if (left.Exp is Never) return Never();
            
            var right = Read(x.Right.Exp);
            if (right.Exp is Never) return Never();

            switch (left.Exp, right.Exp)
            {
                case (Int l, Int r):
                    return (l.Value == r.Value)
                        ? Return(l)
                        : Never();

                case (True _, True _):
                    return Return(new True());

                default: return Never();
            };

            Scope Return(Exp val)
                => new Scope(val, left.Binds.Combine(right.Binds));
        }
    }
}