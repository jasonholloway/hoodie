using System;

namespace Varna
{
    public class Exp
    {
        public static implicit operator Exp(int i) => new Int(i);
        public static implicit operator Exp(string s) => new LeafExp(s);
        
        public static Exp operator ==(Exp left, Exp right) => new EqualsExp(left, right);
        public static Exp operator !=(Exp left, Exp right) => new NotEqualsExp(left, right);

        public static Exp operator &(Exp left, Exp right) => Ops.And(left, right);
        public static Exp operator |(Exp left, Exp right) => Ops.Or(left, right);
        public static Exp operator <(Exp left, Exp right) => null;
        public static Exp operator >(Exp left, Exp right) => new GreaterThanExp(left, right);
        public static Exp operator <=(Exp left, Exp right) => null;
        public static Exp operator >=(Exp left, Exp right) => null;
        
        public static Exp operator +(Exp left, Exp right) => new AddExp(left, right);
    }
    
        
    
    public sealed class Never : LeafExp
    {
        public Never() : base(null)
        { }
    }
    
    public class True : LeafExp
    {
        public True() : base(true)
        { }
    }
    
    public sealed class Int : LeafExp
    {
        public readonly int Value;

        public Int(int value) 
            : base(value)
        {
            Value = value;
        }
    }


    public class Var : Exp
    {
        public readonly string Name;

        public Var(string name)
        {
            Name = name;
        }
    }

    public class LeafExp : Exp
    {
        public readonly object Raw;

        public LeafExp(object raw)
        {
            Raw = raw;
        }
    }

    public class AddExp : BinaryExp
    {
        public AddExp(Scope left, Scope right) 
            : base(left, right)
        {}
    }

    public class EqualsExp : BinaryExp
    {
        public EqualsExp(Scope left, Scope right) 
            : base(left, right)
        {}
    }
    
    public class NotEqualsExp : BinaryExp
    {
        public NotEqualsExp(Scope left, Scope right) 
            : base(left, right)
        {}
    }
    
    public class GreaterThanExp : BinaryExp
    {
        public GreaterThanExp(Scope left, Scope right) 
            : base(left, right)
        {}
    }
    
    public class BindExp : Exp
    {
        public readonly Var Var;
        public readonly Scope Exp;

        public BindExp(Var @var, Scope exp)
        {
            Var = var;
            Exp = exp;
        }
    }

    public class AndExp : BinaryExp
    {
        public AndExp(Scope left, Scope right) 
            : base(left, right)
        {}
    }
    
    public class OrExp : BinaryExp
    {
        public OrExp(Scope left, Scope right) 
            : base(left, right)
        {}
    }
    
    public class BinaryExp : Exp
    {
        public readonly Scope Left;
        public readonly Scope Right;

        public BinaryExp(Scope left, Scope right)
        {
            Left = left;
            Right = right;
        }
    }

    public class BoolExp : Exp
    {
        public readonly bool Val;

        public BoolExp(bool val)
        {
            Val = val;
        }
    }
}