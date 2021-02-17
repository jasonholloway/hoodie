namespace Varna
{
    public static class Ops
    {

        public static Exp True => new BoolExp(true);
        public static Exp False => new BoolExp(false);

        public static Exp Const(int i) => null;

        public static Exp And(Exp left, Exp right)
            => new AndExp(left, right);
        
        public static Exp Or(Exp left, Exp right)
            => new OrExp(left, right);

        public static Exp Bind(Var var, Exp exp)
            => new BindExp(var, exp);
        
        
        public static Exp And(params Exp[] exps) => null;
        public static Exp Or(params Exp[] exps) => null;
    }
}