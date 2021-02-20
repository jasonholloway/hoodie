using System;
using System.Collections.Generic;

namespace Varna
{
    internal class ScopeComparer : IEqualityComparer<Scope>, IEqualityComparer<Exp>
    {
        public static readonly IEqualityComparer<Exp> Exp = new ScopeComparer();
        public static readonly IEqualityComparer<Scope> Scope = new ScopeComparer();
            
        public bool Equals(Scope x, Scope y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Binds.Equals(y.Binds) && Equals(x.Exp, y.Exp) && Equals(x.More, y.More);
        }

        public int GetHashCode(Scope obj)
            => HashCode.Combine(
                obj.Binds, 
                GetHashCode(obj.Exp), 
                obj.More);
            
            
        public bool Equals(Exp x, Exp y)
            => x.GetType() == y.GetType()
               && _Equals((dynamic)x, (dynamic)y);

        bool _Equals(True l, True r)
            => true;

        bool _Equals(Int l, Int r)
            => l.Value == r.Value;
            
        bool _Equals(Var l, Var r)
            => l.Name == r.Name;
        
        bool _Equals(OrExp l, OrExp r)
            => l.Scopes.Equals(r.Scopes);

        bool _Equals(BinaryExp l, BinaryExp r)
            => Equals(l.Left, r.Left) 
               && Equals(l.Right, r.Right);
            

        public int GetHashCode(Exp x)
            => _GetHashCode((dynamic)x);

        int _GetHashCode(True x)
            => typeof(True).GetHashCode();

        int _GetHashCode(Int x)
            => HashCode.Combine(
                x.GetType(), 
                x.Value); 
            
        int _GetHashCode(Var x)
            => HashCode.Combine(
                x.GetType(), 
                x.Name);

        int _GetHashCode(OrExp x)
            => HashCode.Combine(
                typeof(OrExp),
                x.Scopes);
            
        int _GetHashCode(BinaryExp obj)
            => HashCode.Combine(
                obj.GetType(), 
                GetHashCode(obj.Left), 
                GetHashCode(obj.Right));
    }
}