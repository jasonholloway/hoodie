using System;
using System.Collections.Immutable;

namespace Varna
{
    public class Scope
    {
        public readonly ImmutableDictionary<string, Exp> Binds;
        public readonly Exp Exp;
        public readonly Func<Scope> More;

        public Scope(Exp exp = null, ImmutableDictionary<string, Exp> binds = null, Func<Scope> more = null)
        {
            Exp = exp ?? new Never();
            Binds = binds ?? ImmutableDictionary<string, Exp>.Empty;
            More = more;
        }
        
        public static implicit operator Scope(Exp exp)
            => new Scope(exp);
        
        public Scope Get(string name = null)
            => new Scope(
                name == null
                    ? Exp
                    : (
                        Binds.TryGetValue(name, out var found)
                            ? found
                            : new Never()
                    ),
                Binds, 
                More);

        public object Raw()
            => Exp switch
            {
                LeafExp c => c.Raw,
                _ => throw new Exception("Not raw!")
            };
    }
    
    
    // public class Context
    // {
    //     public readonly Scope Current;
    //     public readonly Func<Context> More;
    //
    //     public ImmutableDictionary<string, Exp> Binds => Current.Binds;
    //     public Exp Exp => Current.Exp;
    //
    //     public Context(Scope current = null, Func<Context> more = null)
    //     {
    //         Current = current ?? new Scope();
    //         More = more;
    //     }
    //
    //     public Context(ImmutableDictionary<string, Exp> binds = null, Exp exp = null, Func<Context> more = null)
    //         : this(new Scope(binds, exp), more) {}
    //
    //     public Context Get(string name = null)
    //         => new Context(Current.Binds,
    //             name == null
    //                 ? Current.Exp
    //                 : (
    //                     Current.Binds.TryGetValue(name, out var found)
    //                         ? found
    //                         : new Never()
    //                 ));
    //
    //     public object Raw()
    //         => Current.Exp switch
    //         {
    //             LeafExp c => c.Raw,
    //             _ => throw new Exception("Not raw!")
    //         };
    // }
}