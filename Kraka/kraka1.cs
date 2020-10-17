using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NUnit.Framework;

namespace kraka1
{
    public abstract class Val
    {
        public static Val Any = new Any();
        public static Int Int(int i) => new Int(i);
        public static Never Never = new Never();
        public static Number Number = new Number();
        public static Range IntRange(int? min = null, int? max = null)
            => new Range(min, max);
        
        public static Val Combine(Val v1, Val v2)
        {
            if (v1.Equals(v2)) return v1;

            return (v1, v2) switch
            {
                (Any _, _) => v2,
                (_, Any _) => v1,
                (Never _, _) => Never,
                (_, Never _) => Never,
                (Number _, Int _) => v2,
                (Int _, Number _) => v1,
                (Range _, Number _) => v1,
                (Number _, Range _) => v2,
                (Range r, Int i) => i.Value <= r.Max && i.Value >= r.Min ? (Val)i : Never,
                (Int i, Range r) => i.Value <= r.Max && i.Value >= r.Min ? (Val)i : Never,
                (Range r1, Range r2) => Combine(r1, r2),
                _ => Never
            };

            Val Combine(Range r1, Range r2)
            {
                var min = Math.Max(r1.Min, r2.Min);
                var max = Math.Min(r1.Max, r2.Max);
                return min == max
                    ? Int(min) 
                    : min < max
                        ? (Val)IntRange(min, max)
                        : Never;
            }
        }
    }

    public class Number : Val
    {
        public override string ToString()
            => "Number";
    }

    public class Range : Val, IEquatable<Range>
    {
        public readonly int Min;
        public readonly int Max;

        public Range(int? min, int? max)
        {
            Min = min ?? int.MinValue;
            Max = max ?? int.MaxValue;
        }

        public override string ToString()
            => $"{Min}<{Max}";

        public bool Equals(Range other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Min == other.Min && Max == other.Max;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Range) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Min, Max);
        }
    }

    public class Any : Val
    {
        public override string ToString()
            => "Any";
    }

    public class Never : Val
    {
        public override string ToString()
            => "Never";
    }

    public class Int : Val, IEquatable<Int>
    {
        public readonly int Value;

        public Int(int value)
        {
            Value = value;
        }
        
        public override string ToString()
            => Value.ToString();

        public bool Equals(Int other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Int) obj);
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
    
    public class Node
    {
        readonly Func<Graph, Val, (Graph, Val)> _fn;

        public Node(Func<Graph, Val, (Graph, Val)> fn)
        {
            _fn = fn;
        }

        public (Graph, Val) Impel(Graph g, Val v) => _fn(g, v);
    }

    public static class GraphExtensions
    {
        public static (Graph, Val) Bind(this (Graph, Val) gv, Node n1, Node n2) 
            => gv.Item1.Bind(n1, n2);

        public static Val Sample(this (Graph, Val) gv, Node node)
            => gv.Item1.Sample(node);
    }

    public readonly struct Graph
    {
        private readonly ImmutableDictionary<Node, Node> Binds;

        public Graph(IDictionary<Node, Node> binds = null)
        {
            Binds = binds?.ToImmutableDictionary() ?? ImmutableDictionary<Node, Node>.Empty;
        }

        public (Graph, Val) Bind(Node n1, Node n2)
            => new Graph(Binds
                .Add(n1, n2)
                .Add(n2, n1))
                .Propagate(n1);

        private (Graph, Val) Propagate(Node node)
        {
            var g1 = this;
            var v1 = Val.Any;

            var (g2, v2) = node.Impel(g1, v1);

            if (Binds.TryGetValue(node, out var other))
            {
                var (g3, v3) = other.Impel(g2, v2);
                var v4 = Val.Combine(v2, v3);
                return (g3, v4);
            }
            else
            {
                return (g2, v2);
            }
        }

        public Val Sample(Node node)
        {
            var (_, val) = Propagate(node);
            return val;
        }
        
        //Unix: A Programmer's Paradise(?)

        public static Graph Empty = new Graph(null);
    }
    
    public class Tests
    {
        [Test]
        public void Bind_1And1()
        {
            var m1 = new Constant(1);
            var m2 = new Constant(1);

            var (_, v) = Graph.Empty
                .Bind(m1.Node, m2.Node);

            Assert.That(v, Is.EqualTo(Val.Int(1)));
        }
        
        [Test]
        public void Bind_1And2()
        {
            var m1 = new Constant(1);
            var m2 = new Constant(2);

            var (_, v) = Graph.Empty
                .Bind(m1.Node, m2.Node);
            
            Assert.That(v, Is.EqualTo(Val.Never));
        }
        
        [Test]
        public void Bind_1AndAny()
        {
            var m1 = new Constant(1);
            var m2 = new Constant(Val.Any);

            var (_, v) = Graph.Empty
                .Bind(m1.Node, m2.Node);

            Assert.That(v, Is.EqualTo(Val.Int(1)));
        }
        
        [Test]
        public void Bind_AnyAndNever()
        {
            var m1 = new Constant(Val.Any);
            var m2 = new Constant(Val.Never);

            var (_, v) = Graph.Empty
                .Bind(m1.Node, m2.Node);

            Assert.That(v, Is.EqualTo(Val.Never));
        }
        
        [Test]
        public void Bind_NumberAnd1()
        {
            var m1 = new Constant(Val.Number);
            var m2 = new Constant(Val.Int(1));

            var (_, v) = Graph.Empty
                .Bind(m1.Node, m2.Node);

            Assert.That(v, Is.EqualTo(Val.Int(1)));
        }
        
        [Test]
        public void Bind_IntRangeAnd1()
        {
            var m1 = new Constant(Val.IntRange(1, 10));
            var m2 = new Constant(Val.Int(1));

            var (_, v) = Graph.Empty
                .Bind(m1.Node, m2.Node);

            Assert.That(v, Is.EqualTo(Val.Int(1)));
        }
        
        [Test]
        public void Bind_IntRangeAnd100()
        {
            var m1 = new Constant(Val.IntRange(1, 10));
            var m2 = new Constant(Val.Int(100));

            var (_, v) = Graph.Empty
                .Bind(m1.Node, m2.Node);

            Assert.That(v, Is.EqualTo(Val.Never));
        }
        
        [Test]
        public void Bind_IntRanges_Overlapping()
        {
            var m1 = new Constant(Val.IntRange(1, 10));
            var m2 = new Constant(Val.IntRange(8, 8000));

            var (_, v) = Graph.Empty
                .Bind(m1.Node, m2.Node);

            Assert.That(v, Is.EqualTo(Val.IntRange(8, 10)));
        }
        
        [Test]
        public void Bind_IntRanges_Distinct()
        {
            var m1 = new Constant(Val.IntRange(1, 10));
            var m2 = new Constant(Val.IntRange(80, 8000));

            var (_, v) = Graph.Empty
                .Bind(m1.Node, m2.Node);

            Assert.That(v, Is.EqualTo(Val.Never));
        }
        
        
        public abstract class Machine {}

        public class Constant : Machine
        {
            public readonly Node Node;

            public Constant(Val val)
            {
                Node = new Node((g, _) => (g, val));
            }

            public Constant(int value) : this(Val.Int(value))
            { }
        }

        public class Add : Machine
        {
            public readonly Node Left;
            public readonly Node Right;
            public readonly Node Result;

            Val _leftVal = Val.Any;
            Val _rightVal = Val.Any;
            Val _resultVal = Val.Any;

            public Add()
            {
                Left = new Node((g, v) =>
                {
                    switch (v, _rightVal, _resultVal)
                    {
                        case (Int l, Int r, Int o): 
                            var o1 = l.Value + r.Value;
                            //now we have this new out value
                            //and should propagate this forwards
                            //and receive back

                            var r1 = o.Value - l.Value;
                            //...

                            var l1 = o.Value - r.Value;
                            //...
                            
                            return (g, l);
                        
                        case (Int l, Int right, Range result): return (g, l);
                        
                        case (Int r, Int right, Any result): return (g, r);
                        
                        
                        case (Any _, Any _, Any _): return (g, Val.Any);
                        default: return (g, Val.Never);
                    }
                });
                
                //need a simpler case
                //any/(true^false)/never
                //though this becomes too simple
                //
                //are there any unary operators that take ints 
                //(-) 
                //
                // a & b = c 
                //
                // Any & Any = Any 
                // True & Any = Any
                // False & Any = Any
                // Any & True = Any
                // Any & False = Any
                // True & False = False
                // True & True = True
                // False & False = False
                // False & True = False
                // Never & Never = Never
                // Never & True = Never
                // Never & False = Never
                //
                // Never is a cross-cutting scheme, that dovetails with disjunctions
                // should be represented by below:
                // 
                // [True, False] & [True] = [True, False]
                // [] & [True] = []
                //
                // Meanwhile Any is... an abstraction
                // it's a shortcut rather than listing each and every possibility - is one point of view
                // really both schemes are right in their way, and there has to be some mediation between them
                // as soon as we can we need to abstract and rely on special rules for that abstraction
                //
                // A range can be unpacked, and a collection of elements can in some cases be packed up for simplification
                // A simple case would be a limited range of ints - -3:3 maybe, 3-bit addition
                // we could do addition like this, with disjunctions and never, then we'd have to battle the abstraction of ranges
                //
                //
                //
                
                Right = new Node((g, v) =>
                {
                    switch (_leftVal, _resultVal)
                    {
                        case (Any _, Any _): return (g, v);
                        default: return (g, Val.Never);
                    }
                });
                
                Result = new Node((g, v) =>
                {
                    switch (_leftVal, _rightVal)
                    {
                        case (Any _, Any _): return (g, v);
                        default: return (g, Val.Never);
                    }
                });
            }
        }
        
        [Test]
        public void Adds_1And12()
        {
            var x = new Constant(1);
            var y = new Constant(12);
            var add = new Add();

            var result = Graph.Empty
                .Bind(add.Left, x.Node)
                .Bind(add.Right, y.Node)
                .Sample(add.Result);

            Assert.That(result, Is.EqualTo(Val.Int(13)));
        }
        
    }
}