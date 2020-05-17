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

    public class GroupMonoid<N, V> : IMonoid<Group<N, V>>
    {
        readonly IMonoid<V> _monoidV;

        public GroupMonoid(IMonoid<V> monoidV)
        {
            _monoidV = monoidV;
        }
        
        public Group<N, V> Zero 
            => Group.From(Enumerable.Empty<N>(), _monoidV.Zero);
        
        public Group<N, V> Combine(Group<N, V> left, Group<N, V> right)
            => new Group<N, V>(
                left.Nodes.Union(right.Nodes),
                ImmutableHashSet<int>.Empty, 
                _monoidV.Combine(left.Value, right.Value));
    }

    public class GroupMapMonoid<N, V> : IMonoid<Map<N, V>>
    {
        readonly IMonoid<Group<N, V>> _monoidGroup;

        public GroupMapMonoid(IMonoid<Group<N, V>> monoidGroup)
        {
            _monoidGroup = monoidGroup;
        }
        
        public Map<N, V> Zero => Map<N, V>.Empty;

        public Map<N, V> Combine(Map<N, V> left, Map<N, V> right)
        {
            var nodes = right.Index.Keys;
            
            var leftGroups = nodes.SelectMany(n => left[n]).ToArray();
            var rightGroups = nodes.SelectMany(n => right[n]);
            
            //instead of gathering everything up into one big heap up front
            //need to explore the graph; we choose the right disjunct
            //and in choosing it, we could even simplify the right hand graph
            //
            //
            //

            var leftMap1 = leftGroups.Aggregate(left, (m, g) => m.Remove(g));

            var bigGroup = leftGroups.Concat(rightGroups)
                .Aggregate(_monoidGroup.Zero, _monoidGroup.Combine);

            return leftMap1; //.Add(GroupMap.Lift(bigGroup));
        }
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
            => (Chars != null ? Chars.Aggregate(1, (ac, c) => ac + c.GetHashCode()).GetHashCode() : 0);
    }
}