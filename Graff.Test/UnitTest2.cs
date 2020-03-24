using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NUnit.Framework;
using Shouldly;

namespace Graff.Test2
{
    using static GraphOps;
    
    public class Tests
    {
        [Test]
        public void SuperSimpleSystem()
        {
            var graph =
                from varX in Var("x")
                from _ in Assert(GreaterThan(varX, 3))
                select varX;

            var (g, x) = graph(new GraphState());
            
            var domain = g.GetDomain(x);
            domain.ShouldNotBeNull();
        }
    }
    
    //variables are nothing but references to ports
    //except - multiple ports can be united by a variable
    //they are like a self-equal constraint, with however many ports
    //they are a kind of constraint; and like any constraint they have their own encapsulated logic of propagation

    public class Port
    {
        public readonly string Name;

        public Port(string name)
        {
            Name = name;
        }

        #region Conversions
        
        public static implicit operator Port(Var @var)
            => @var.Port;

        public static implicit operator Port(Const @const)
            => @const.Port;
        
        public static implicit operator Port(Domain domain)
            => new Const(domain);

        public static implicit operator Port(int value)
            => new IntDomain(); 
        
        public static implicit operator Port(bool value)
            => new BoolDomain();

        #endregion
    }
    
    
    public abstract class Domain {}
    public class AnyDomain : Domain {}
    public class NeverDomain : Domain {}
    public class IntDomain : Domain {}
    public class BoolDomain : Domain {}
    

    public class Binding
    {
        private readonly ImmutableHashSet<Port> _ports;
        public readonly Domain Domain;

        public Binding(ImmutableHashSet<Port> ports, Domain domain)
        {
            _ports = ports;
            Domain = domain;
        }

        public Binding() 
            : this(ImmutableHashSet<Port>.Empty, new AnyDomain())
        { }

        public IEnumerable<Port> Ports => _ports;

        public Binding SetDomain(Domain domain)
        {
            throw new NotImplementedException();
        }

        public Binding AddPort(Port port)
        {
            throw new NotImplementedException();
        }
    }
    

    public abstract class Relation
    {
    }

    public class Var : Relation
    {
        public readonly Port Port;

        public Var()
        {
            Port = new Port("Port");
        }
    }

    public class Const : Relation
    {
        public readonly Port Port;

        public Const(Domain domain)
        {
            Port = new Port("Port");
        }
    }

    public class AreEqualConstraint : Relation
    {
        public readonly Port Left;
        public readonly Port Right;
        public readonly Port Result;

        public AreEqualConstraint()
        {
            Left = new Port(nameof(Left));
            Right = new Port(nameof(Right));
            Result = new Port(nameof(Result));
        }
    }

    public class GreaterThanConstraint : Relation
    {
        public readonly Port Left;
        public readonly Port Right;
        public readonly Port Result;

        public GreaterThanConstraint()
        {
            Left = new Port(nameof(Left));
            Right = new Port(nameof(Right));
            Result = new Port(nameof(Result));
        }
    }

    public class IsNumberConstraint : Relation
    {
        public readonly Port Inner;
        public readonly Port Result;

        public IsNumberConstraint()
        {
            Inner = new Port(nameof(Inner));
            Result = new Port(nameof(Result));
        }
    }

    public delegate (GraphState, TVal) Graph<TVal>(GraphState graph = default);

    public class GraphState
    {
        private readonly ImmutableDictionary<string, Var> _vars;
        private readonly ImmutableDictionary<Port, Binding> _binds;

        private GraphState(ImmutableDictionary<string, Var> vars = default, ImmutableDictionary<Port, Binding> binds = default)
        {
            _vars = vars;
            _binds = binds;
        }
        
        public GraphState() 
            : this(ImmutableDictionary<string, Var>.Empty, ImmutableDictionary<Port, Binding>.Empty)
        { }

        public GraphState Bind(params Port[] ports)
        {
            
            
            
            throw new NotImplementedException();
        }

        public GraphState Pin(Port port, Domain domain)
        {
            throw new NotImplementedException();
        }

        public (GraphState, Var) SummonVar(string name)
        {
            if (_vars.TryGetValue(name, out var found))
            {
                return (this, found);
            }

            var @var = new Var();
            return (
                new GraphState(
                    _vars.SetItem(name, @var), 
                    _binds
                    ),
                @var
            );
        }

        public Domain GetDomain(Port port)
        {
            throw new NotImplementedException();
        }
    }

    public static class GraphOps
    {
        public static Graph<Port> IsNumber(Port port)
            => graph =>
            {
                var isNumber = new IsNumberConstraint();
                return (
                    graph.Bind(port, isNumber.Inner), 
                    isNumber.Result
                );
            };
        
        public static Graph<Var> Var(string name)
            => graph => graph.SummonVar(name);

        public static Graph<Port> AreEqual(Port p1, Port p2)
            => graph =>
            {
                var areEqual = new AreEqualConstraint();
                return (
                    graph
                        .Bind(p1, areEqual.Left)
                        .Bind(p2, areEqual.Right),
                    areEqual.Result
                );
            };
        
        public static Graph<Port> GreaterThan(Port p1, Port p2)
            => graph =>
            {
                var greaterThan = new GreaterThanConstraint();
                return (
                    graph
                        .Bind(p1, greaterThan.Left)
                        .Bind(p2, greaterThan.Right),
                    greaterThan.Result
                );
            };

        public static Graph<T> Pin<T>(Port p, T value)
            => graph => (
                graph.Pin(p, new BoolDomain()),
                value
            );
        
        public static Graph<object> Sample(Port port)
            => throw new NotImplementedException();

        public static Graph<bool> Assert(Port port)
            => Pin(port, true);

        public static Graph<bool> Assert(Graph<Port> graph)
            => from value in graph
               from result in Assert(value)
               select result;
    }
    

    public static class GraphExtensions
    {
        public static Graph<TResult> Select<TSource, TResult>(this Graph<TSource> source, Func<TSource, TResult> select)
            => graph =>
            {
                var (g, v) = source(graph);
                return (g, select(v));
            };

        public static Graph<TTo> SelectMany<TFrom, TVia, TTo>(this Graph<TFrom> source, Func<TFrom, Graph<TVia>> collectionSelector, Func<TFrom, TVia, TTo> resultSelector)
            => graph =>
            {
                var (g1, v1) = source(graph);
                var (g2, v2) = collectionSelector(v1)(g1);
                return (g2, resultSelector(v1, v2));
            };

    }
}