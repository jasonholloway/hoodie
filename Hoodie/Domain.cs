using System;

namespace Hoodie
{
    public abstract class Domain : IEquatable<Domain>
    {
        private static int _nextId = 0;
        private int _id = _nextId++;

        public static implicit operator Domain(bool value)
            => value 
                ? (BoolDomain)new TrueDomain() 
                : new FalseDomain();

        public static implicit operator Domain(int value)
            => new IntDomain(); //TODO 

        public bool Equals(Domain other)
            => _id == other._id; //TODO should fallback on proper equality
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