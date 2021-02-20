using System;

namespace Varna
{
    public class Scope
    {
        public readonly Binds Binds;
        public readonly Exp Exp;
        public readonly Func<Scope> More;

        public Scope(Exp exp, Binds binds, Func<Scope> more = null)
        {
            if (binds == null)
            {
                Exp = new Never();
                More = null;
            }
            else
            {
                Exp = exp;
                More = more;
                Binds = binds;
            }
        }
        
        public static implicit operator Scope(Exp exp)
            => new Scope(exp, exp is Never ? null : Binds.Empty);
        
        public Scope Get(string name = null)
            => new Scope(
                name == null
                    ? Exp
                    : Binds?.Get(name),
                Binds, 
                More);

        public object Raw()
            => Exp switch
            {
                LeafExp c => c.Raw,
                _ => throw new Exception("Not raw!")
            };
    }
}