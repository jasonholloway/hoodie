using System.Collections.Generic;

namespace probs
{
    using static Ops;

    public static class Ops
    {
        public static void Relate(IAtom atom1, IAtom atom2)
        {

        }
        
        public static Atom<T> Atom<T>(T val)
            => new Atom<T>(val);
    }

    
    public interface IAtom {}
    
    public struct Atom<T> : IAtom
    {
        public readonly T Value;

        public Atom(T value)
        {
            Value = value;
        }
    }

    //but atoms aren't independent! they're little handles of possibility

    public abstract class Data
    {
        public ISet<Atom<string>> CountryCodes = new HashSet<Atom<string>>
        {
            Atom("UK"),
            Atom("GB"),
            Atom("IE"),
            Atom("DE"),
            Atom("AU")
        };

        public ISet<Atom<string>> Carriers = new HashSet<Atom<string>>
        {
            Atom("AUSPO"),
            Atom("UPS"),
            Atom("HERMES"),
            Atom("TNT")
        };
    }
    
    
    
    internal class Program
    {
        public static void Main(string[] args)
        {
            var consignment = new Consignment
            {
                From = new Address(),
                To = new Address(),
                Carrier = new CarrierRef("blah")
            };
        }
    }

    
    public class Consignment
    {
        public Address From { get; set; }
        public Address To { get; set; }
        public CarrierRef Carrier { get; set; }
    }

    public class Address
    {
        public string CountryCode { get; set; }
    }

    public struct CarrierRef
    {
        public readonly string Reference;

        public CarrierRef(string reference)
        {
            Reference = reference;
        }
    }

}