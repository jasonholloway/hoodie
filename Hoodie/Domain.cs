namespace Hoodie
{
    public abstract class Domain
    {
        public static implicit operator Domain(bool value)
            => value 
                ? (BoolDomain)new TrueDomain() 
                : new FalseDomain();

        public static implicit operator Domain(int value)
            => new IntDomain(); //TODO 
    }
    
    public class AnyDomain : Domain {}
    public class NeverDomain : Domain {}
    public class IntDomain : Domain {}
    public class BoolDomain : Domain {}
    public class TrueDomain : BoolDomain {}
    public class FalseDomain : BoolDomain {}

    public static class Domains
    {
        public static readonly Domain Any = new AnyDomain();
        public static readonly Domain Never = new NeverDomain();
        public static readonly Domain Bool = new BoolDomain();
    }
}