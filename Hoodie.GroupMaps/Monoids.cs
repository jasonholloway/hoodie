using System;
using System.Collections.Immutable;
using System.Linq;

namespace Hoodie.GroupMaps
{
    public interface IMonoid<T>
    {
        T Zero { get; }
        T Combine(T left, T right);
    }

    public class StringMonoid : IMonoid<string>
    {
        public string Zero => "";
        public string Combine(string left, string right)
            => left + right;
    }



    public class SymMonoid : IMonoid<Sym>
    {
        public Sym Zero { get; }
            = new Sym(ImmutableSortedSet<char>.Empty);

        public Sym Combine(Sym left, Sym right)
            => new Sym(left.Chars.Union(right.Chars));
    }

    public struct Sym : IEquatable<Sym>
    {
        public readonly ImmutableSortedSet<char> Chars;

        internal Sym(ImmutableSortedSet<char> chars)
        {
            Chars = chars;
        }

        public override string ToString()
            => string.Join("", Chars);

        public static Sym From(char @char)
            => new Sym(ImmutableSortedSet<char>.Empty.Add(@char));

        public static Sym From(string @string)
            => new Sym(@string.Aggregate(
                ImmutableSortedSet<char>.Empty, 
                (ac, c) => ac.Add(c)));

        public static implicit operator Sym(char @char)
            => From(@char);

        public static bool operator ==(Sym left, Sym right) => left.Equals(right);
        public static bool operator !=(Sym left, Sym right) => !(left == right);


        public bool Equals(Sym other)
            => Chars.SetEquals(other.Chars);

        public override bool Equals(object obj) 
            => obj is Sym other && Equals(other);

        public override int GetHashCode() 
            => (Chars != null ? Chars.Aggregate(1, (ac, c) => ac + c.GetHashCode() * 77 + 93) : 0);
    }
}