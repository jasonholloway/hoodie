using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;

//Unix: A Bit Like DotNet

namespace kraka2
{
    public class Int : Any, IEquatable<Int>
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

    public class Choice : Any, IEquatable<Choice>
    {
        public readonly Val[] Values;

        public Choice(params Val[] values)
        {
            Values = values.Where(v => !v.Equals(Never)).ToArray(); //dodge
        }
        
        public Choice(IEnumerable<Val> vals)
            : this(vals.ToArray())
        { }

        public override string ToString()
            => $"[{string.Join(",", Values.Select(v => v.ToString()))}]";

        public bool Equals(Choice other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Equals(Values, other.Values)) return true;
            return Values.SequenceEqual(other.Values);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Choice) obj);
        }

        public override int GetHashCode()
        {
            return (Values != null ? Values.GetHashCode() : 0);
        }
    }

    public class Any : Val
    {
        public override string ToString()
            => "Any";
    }
    
    
    public abstract class Val
    {
        public static Any Any => new Any();
        public static Int Int(int i) => new Int(i);
        public static Choice Choice(params Val[] vals) => new Choice(vals);
        public static Choice Never => Choice();
        
        public static Val Combine(Val v1, Val v2)
        {
            if (v1.Equals(v2)) return v1;
            
            var r = v1.GetType().IsInstanceOfType(v2)
                ? _Combine((dynamic)v1, (dynamic)v2)
                : _Combine((dynamic)v2, (dynamic)v1);

            return r;
        }

        static Val _Combine(Choice l, Choice r)
            => new Choice((
                from lv in l.Values
                from rv in r.Values
                select Combine(lv, rv)
                ));

        static Val _Combine(Any l, Choice r)
            => _Combine(Choice(l), r);

        static Val _Combine(Int l, Int r)
            => l.Value == r.Value ? (Val)l : Never;

        static Val _Combine(Any l, Any r) => r;
    }


    
    public class Node
    {
        readonly string _name;
        readonly Func<Graph, Val, (Graph, Val)> _fn;

        public Node(string name, Func<Graph, Val, (Graph, Val)> fn)
        {
            _name = name;
            _fn = fn;
        }

        public (Graph, Val) Impel(Graph g, Val v) => _fn(g, v);

        public override string ToString()
            => _name;
    }

    public class Bind
    { }

    public readonly struct BindState
    {
        public readonly ImmutableHashSet<Node> Nodes;
        public readonly Val Val;

        private BindState(IEnumerable<Node> nodes, Val val = null)
        {
            Nodes = nodes?.ToImmutableHashSet() ?? ImmutableHashSet<Node>.Empty;
            Val = val ?? Val.Any;
        }
        
        public static BindState Empty => new BindState(null);
        
        public BindState WithNode(Node n)
            => new BindState(Nodes.Add(n), Val);
        
        public BindState WithVal(Val v)
            => new BindState(Nodes, Val.Combine(Val, v));

        public BindState MergeWith(BindState other)
            => new BindState(
                Nodes.Union(other.Nodes),
                Val.Combine(Val, other.Val));

        public override string ToString()
            => $"<{string.Join(", ",Nodes)}> = {Val}";
    }


    public readonly struct Graph
    {
        readonly ImmutableDictionary<Node, Bind> NodeBinds;
        readonly ImmutableDictionary<Bind, BindState> BindStates;

        public Graph(IDictionary<Node, Bind> nodeBinds = null, IDictionary<Bind, BindState> bindStates = null)
        {
            NodeBinds = nodeBinds?.ToImmutableDictionary() ?? ImmutableDictionary<Node, Bind>.Empty;
            BindStates = bindStates?.ToImmutableDictionary() ?? ImmutableDictionary<Bind, BindState>.Empty;
        }

        static int _depth = 0;
        static string Prefix => new string(Enumerable.Range(0, _depth).Select(_ => '\t').ToArray());
        
        private Graph Propagate(Bind b, BindState s)
        {
            TestContext.WriteLine($"{Prefix}PROP {s}");
            _depth++;
            TestContext.Write($"{Prefix}{s.Val}");
            
            var r = s.Nodes.Aggregate(
                (g: this, v: s.Val),
                (ac, n) =>
                {
                    TestContext.WriteLine($" -> {n}");
                    
                    var (g2, v2) = n.Impel(ac.g, ac.v);
                    var v3 = Val.Combine(ac.v, v2);
                    
                    TestContext.Write($"{Prefix}{v3}");
                    
                    return (g2, v3);
                });
            
            TestContext.WriteLine();
            _depth--;
            
            //but this only stores final val
            //Node.Impel should somehow update BindStates...
            
            return new Graph(
                r.g.NodeBinds,
                r.g.BindStates.SetItem(b, s.WithVal(r.v))
                );
        }
        
        //in merging, we should aggregate BindStates
        //
        //

        public Graph MergeWith(Graph other)
            => other.BindStates
                .Aggregate(this,
                    (ac, t) => ac.MergeBind(t.Key, t.Value));
        
        private Graph MergeBind(Bind rightBind, BindState rightState)
        {
            //HORRORS BELOW
            var x = this;
            
            var leftBinds = rightState.Nodes
                .Select(n => x.NodeBinds.TryGetValue(n, out var found) ? found : null)
                .Where(n => n != null)
                .ToArray();

            var leftStates = leftBinds
                .SelectMany(b => x.BindStates.TryGetValue(b, out var found) ? new[] { found } : new BindState[0])
                .ToArray();

            var allStates = leftStates.Concat(new[] {rightState});

            var allNodes = allStates.SelectMany(s => s.Nodes);

            var bind = leftBinds.Concat(new[] {rightBind}).First();
            var state = allStates.Aggregate(BindState.Empty, (ac, s) => ac.MergeWith(s));

            return new Graph(
                allNodes
                    .Aggregate(NodeBinds, (ac, n) => ac.SetItem(n, bind)),
                BindStates
                    .SetItem(bind, state)
                )
                .Propagate(bind, state);
        }

        public Graph Bind(IEnumerable<Node> nodes, Val v = null)
        {
            var b = new Bind();

            var g2 = new Graph(
                nodes.Aggregate(
                    ImmutableDictionary<Node, Bind>.Empty,
                    (ac, n) => ac.Add(n, b)),
                ImmutableDictionary<Bind, BindState>.Empty
                    .Add(b, 
                        nodes.Aggregate(
                            BindState.Empty,
                            (ac, n) => ac.WithNode(n))
                        .WithVal(v ?? Val.Any))
            );
            
            return MergeWith(g2);
        }
        
        public Graph Bind(Node node, Val val)
            => Bind(new[] { node }, val);
        
        public Graph Bind(Node node1, Node node2, Val val = null)
            => Bind(new[] { node1, node2 }, val);
        
        public Val GetVal(Node node)
        {
            if (NodeBinds.TryGetValue(node, out var b) 
                && BindStates.TryGetValue(b, out var s))
            {
                return s.Val;
            }

            return Val.Any;
        }


        public static Graph Empty => new Graph(null);
    }
    
    public abstract class Machine {}

    public class Constant : Machine
    {
        public readonly Node Node;

        public Constant(Val val)
        {
            Node = new Node(
                $"Const({val}):N",
                (g, _) => (g, val));
        }

        public Constant(int value) : this(Val.Int(value))
        { }
    }
    
    public class Abs : Machine
    {
        public readonly Node Left;
        public readonly Node Right;

        public Abs()
        {
            Left = new Node(
                "Abs:L",   
                (g, v) =>
                {
                    switch (v)
                    {
                        case Int i:
                            var g2 = g.Bind(Right, Val.Int(Math.Abs(i.Value)));
                            return (g2, v);
                        
                        case Any _:
                            return (g, g.GetVal(Left));
                        
                        default:
                            throw new InvalidOperationException();
                    }
                });
            
            Right = new Node(
                "Abs:R",
                (g, v) =>
                {
                    switch (v)
                    {
                        case Int i when i.Value >= 0: {
                            var g2 = g.Bind(Left, Val.Choice(Val.Int(-i.Value), i));
                            return (g2, v);
                        }

                        case Int i:
                            throw new InvalidOperationException();

                        case Any _: {
                            //what's the value on Left?

                            var v2 = g.GetVal(Right);
                            
                            TestContext.WriteLine($"r {v2}");
                            return (g, v2);
                        }

                        default:
                            throw new InvalidOperationException();
                    }
                });
        }
    }

    public class Tests
    {
        [Test]
        public void Bind_1And1()
        {
            var m1 = new Constant(1);
            var m2 = new Constant(1);

            var v = Graph.Empty
                .Bind(m1.Node, m2.Node)
                .GetVal(m1.Node);

            Assert.That(v, Is.EqualTo(Val.Int(1)));
        }
        
        [Test]
        public void Bind_1And2()
        {
            var m1 = new Constant(1);
            var m2 = new Constant(2);

            var v = Graph.Empty
                .Bind(m1.Node, m2.Node)
                .GetVal(m1.Node);
            
            Assert.That(v, Is.EqualTo(Val.Never));
        }
        
        [Test]
        public void Bind_1AndAny()
        {
            var m1 = new Constant(1);
            var m2 = new Constant(Val.Any);

            var v = Graph.Empty
                .Bind(m1.Node, m2.Node)
                .GetVal(m1.Node);

            Assert.That(v, Is.EqualTo(Val.Int(1)));
        }
        
        [Test]
        public void Bind_AnyAndNever()
        {
            var m1 = new Constant(Val.Any);
            var m2 = new Constant(Val.Never);

            var v = Graph.Empty
                .Bind(m1.Node, m2.Node)
                .GetVal(m1.Node);

            Assert.That(v, Is.EqualTo(Val.Never));
        }
        
        
        [Test]
        public void Abs_Forwards()
        {
            var x = new Constant(-13);
            var abs = new Abs();

            var result = Graph.Empty
                .Bind(abs.Left, x.Node)
                .GetVal(abs.Right);

            Assert.That(result, Is.EqualTo(Val.Int(13)));
        }
        
        [Test]
        public void Abs_Backwards()
        {
            var y = new Constant(1);
            var abs = new Abs();
            
            var result = Graph.Empty
                .Bind(abs.Right, y.Node)
                .GetVal(abs.Left);

            Assert.That(result, Is.EqualTo(Val.Choice(Val.Int(-1), Val.Int(1))));
        }
        
        [Test]
        public void Abs_Chained()
        {
            var x = new Constant(7);
            var abs1 = new Abs();
            var abs2 = new Abs();
            
            var result = Graph.Empty
                .Bind(abs1.Left, x.Node)
                .Bind(abs1.Right, abs2.Left)
                .GetVal(abs2.Right);
            
            TestContext.WriteLine(result);

            Assert.That(result, Is.EqualTo(Val.Choice(Val.Int(-1), Val.Int(1))));
        }
        
    }
}