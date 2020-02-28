namespace probs
{
    using static Ops;

    public static class Ops
    {
        public static void Assert(Atom<bool> atom) {}

        public static Atom<bool> In<T>(this Atom<T> atom, params T[] vals) => default;

        public static Atom<bool> Therefore(this Atom atom, Atom other) => default;
    }

    public abstract class Atom
    {
        public static Atom<bool> operator &(Atom me, Atom other) => default;
        public static Atom<bool> operator ^(Atom me, Atom other) => default;
    }

    public class Atom<T> : Atom
    {
        public static Atom<bool> operator ==(Atom<T> me, T val) => default;
        public static Atom<bool> operator !=(Atom<T> me, T val) => default;
    }
    
    internal class Program
    {
        public static void Main(string[] args)
        {
            var env = new Atom<string>();
            Assert(env.In("F1", "F2", "PreProd"));

            var country = new Atom<string>();
            Assert(country.In("UK", "GB", "US", "DE", "AU"));

            var carrier = new Atom<string>();
            Assert(carrier.In("AUSPO", "UPS", "HERMES", "TNT"));
            
            
            Assert((country == "AU") ^ (country == "US"));
            
            Assert((country == "AU").Therefore(carrier == "AUSPO"));
            Assert((country == "US").Therefore(carrier == "UPS"));
            
            Assert(country == "AU");
        }
    }

}