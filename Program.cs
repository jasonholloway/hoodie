using System;
using System.Collections.Generic;

namespace probs
{
    using static Ops;

    public static class Ops
    {
        public static void Assert(Node node) {}

        public static Node In(this Node node, params object[] vals) => default;

        public static Node Therefore(this Node node, Node other) => default;

        public static IEnumerable<object> Eval(this Node node)
        {
            //need to aggregate constraints... yielding narrower and narrower atoms

            return new object[0];
        }
    }

    public class Value
    {
        
        
    }

    public class Node
    {
        public readonly List<Constraint> Constraints = new List<Constraint>();

        public override string ToString()
        {
            return string.Join(", ", this.Eval());
        }

        public static Node operator ==(Node me, object val)
        {
            var result = new Node();
            
            var constraint = new Equals(me, val, result);
            me.Constraints.Add(constraint);
            result.Constraints.Add(constraint);

            return result;
        }

        public static Node operator !=(Node me, object val) => default;
        
        public static Node operator &(Node me, Node other) => default;
        public static Node operator ^(Node me, Node other) => default;
        
    }
    
    public abstract class Constraint {
    }

    public class Equals : Constraint
    {
        public Node Left { get; }
        public object Right { get; }
        public Node Result { get; }

        public Equals(Node left, object right, Node result)
        {
            Left = left;
            Right = right;
            Result = result;
        }
    }

    
    internal class Program
    {
        public static void Main(string[] args)
        {
            var country = new Node();
            Assert(country == "AU");
            
            Console.WriteLine();
            Console.WriteLine($"country: {country}");
            Console.WriteLine();
        }
        
        public static void _Main(string[] args)
        {
            var env = new Node();
            Assert(env.In("F1", "F2", "PreProd"));

            var country = new Node();
            Assert(country.In("UK", "GB", "US", "DE", "AU"));

            var carrier = new Node();
            Assert(carrier.In("AUSPO", "UPS", "HERMES", "TNT"));
            
            Assert((country == "AU") ^ (country == "US"));
            
            Assert((country == "AU").Therefore(carrier == "AUSPO"));
            Assert((country == "US").Therefore(carrier == "UPS"));
            
            Assert(country == "AU");
            
            Console.WriteLine();
            Console.WriteLine($"country: {country}");
            Console.WriteLine();
        }
    }

}