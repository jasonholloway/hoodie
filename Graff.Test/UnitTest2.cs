using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace Graff.Test2
{
    using static GraphOps;
    using static DomainOps;
    
    public class Tests
    {
        [Test]
        public void SuperSimpleSystem()
        {
            var graph =
                from varX in Var("x")
                from _ in Assert(GreaterThan(varX, 3))
                select varX;

            var (g, x) = graph(new Env());
            
            var domain = g.GetDomain(x);
            domain.ShouldNotBeNull();
        }
    }
    
    //variables are nothing but references to ports
    //except - multiple ports can be united by a variable
    //they are like a self-equal constraint, with however many ports
    //they are a kind of constraint; and like any constraint they have their own encapsulated logic of propagation

    public struct DomainChange
    {
        public readonly Domain From;
        public readonly Domain To;

        public DomainChange(Domain @from, Domain to)
        {
            From = @from;
            To = to;
        }
        
        public static implicit operator DomainChange((Domain, Domain) tup)
            => new DomainChange(tup.Item1, tup.Item2);
    }
    

    public delegate DisjunctGraph<Domain> Ripple(Graph<DomainChange> graph);

    public class Port
    {
        public readonly string Name;
        private readonly Ripple _ripple;

        public Port(string name, Ripple ripple)
        {
            Name = name;
            _ripple = ripple;
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


    public abstract class Domain
    {
        public static implicit operator Domain[](Domain domain)
            => new[] {domain};
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
            : this(ImmutableHashSet<Port>.Empty, Domains.Any)
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
            Port = new Port(nameof(Var), 
                incoming => 
                    from _ in incoming
                    select Domains.Any);
        }
    }

    public class Const : Relation
    {
        public readonly Port Port;

        public Const(Domain domain)
        {
            Port = new Port(nameof(Const),
                received =>
                    from d1 in received
                    select d1.From is AnyDomain 
                        ? domain 
                        : Domains.Any);
        }
    }

    public class AreEqualConstraint : Relation
    {
        public readonly Port Left;
        public readonly Port Right;
        public readonly Port Result;

        public AreEqualConstraint()
        {
            Left = new Port(nameof(Left), 
                incoming =>
                    from change in incoming
                    from right in Domain(Right)
                    from result in Domain(Result)
                    from d2 in (right, result) switch 
                    {
                        (_, TrueDomain _) => Set(Right, change.To),
                        (_, FalseDomain _) => Set(Right, Invert(change.To)),
                        (_, BoolDomain _) => Set(Result, change.To),     //if result isn't set, then 
                        _ => SetNever(Right, Result)
                    }
                    select d2);

            Right = new Port(nameof(Right),
                incoming =>
                    from change in incoming    //TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    from right in Domain(Right)
                    from result in Domain(Result)
                    from d2 in (right, result) switch
                    {
                        (_, TrueDomain _) => Set(Left, change.To),
                        (_, FalseDomain _) => Set(Left, change.To),
                        (_, BoolDomain _) => Set(Result, change.To),
                        _ => SetNever(Left, Result)
                    }
                    select d2);

            Result = new Port(nameof(Result),
                incoming =>
                    from change in incoming    //TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    from right in Domain(Right)
                    from result in Domain(Result)
                    from d2 in (right, result) switch
                    {
                        (_, TrueDomain _) => Set(Right, change.To),
                        (_, FalseDomain _) => Set(Right, change.To),
                        (_, BoolDomain _) => Set(Result, change.To),
                        _ => SetNever(Right, Result)
                    }
                    select d2);
        }
    }

    public class GreaterThanConstraint : Relation
    {
        public readonly Port Left;
        public readonly Port Right;
        public readonly Port Result;

        public GreaterThanConstraint()
        {
            Left = new Port(nameof(Left), 
                incoming =>
                    from _ in incoming
                    select Domains.Any);
            
            Right = new Port(nameof(Right), 
                incoming =>
                    from _ in incoming
                    select Domains.Any);
            
            Result = new Port(nameof(Result), 
                incoming =>
                    from _ in incoming
                    select Domains.Any);
        }
    }

    public class IsNumberConstraint : Relation
    {
        public readonly Port Inner;
        public readonly Port Result;

        public IsNumberConstraint()
        {
            Inner = new Port(nameof(Inner),
                incoming =>
                    from _ in incoming
                    select Domains.Any);
            
            Result = new Port(nameof(Result),
                incoming =>
                    from _ in incoming
                    select Domains.Any);
        }
    }

    public delegate (Env, TOut) Graph<TOut>(Env env = default);

    public static class Graph
    {
        public static Graph<TOut> Lift<TOut>(TOut val) 
            => g => (g, val);

        public static Graph<TOut> From<TOut>(Graph<TOut> graph)
            => graph;
    }
    
    
    public class DisjunctGraph<T>
    {
        public readonly Graph<IEnumerable<(Env, T)>> Invoke;

        public DisjunctGraph(Graph<IEnumerable<(Env, T)>> invoke)
        {
            Invoke = invoke;
        }

        public static implicit operator DisjunctGraph<T>(Graph<T> graph)
            => new DisjunctGraph<T>(env =>
            {
                var (env2, v) = graph(env);
                return (env2, Enumerable.Repeat((env2, v), 1));
            });
    }
    

    public class Env
    {
        private readonly ImmutableDictionary<string, Var> _vars;
        private readonly ImmutableDictionary<Port, Binding> _binds;

        private Env(ImmutableDictionary<string, Var> vars = default, ImmutableDictionary<Port, Binding> binds = default)
        {
            _vars = vars;
            _binds = binds;
        }
        
        public Env() 
            : this(ImmutableDictionary<string, Var>.Empty, ImmutableDictionary<Port, Binding>.Empty)
        { }

        public Env Bind(params Port[] ports)
        {
            throw new NotImplementedException();
        }

        public Env Pin(Port port, Domain domain)
        {
            throw new NotImplementedException();
        }

        public (Env, Var) SummonVar(string name)
        {
            if (_vars.TryGetValue(name, out var found))
            {
                return (this, found);
            }

            var @var = new Var();
            return (
                new Env(
                    _vars.SetItem(name, @var), 
                    _binds
                    ),
                @var
            );
        }

        public (Env, IEnumerable<(Env, Domain)>) GetDomain(Port port)
        {
            throw new NotImplementedException();
        }

        public static Env Merge(params Env[] envs)
        {
            throw new NotImplementedException();
        }
    }

    public static class GraphOps
    {
        public static Graph<Port> IsNumber(Port port)
            => env =>
            {
                var isNumber = new IsNumberConstraint();
                return (
                    env.Bind(port, isNumber.Inner), 
                    isNumber.Result
                );
            };
        
        public static Graph<Var> Var(string name)
            => env => env.SummonVar(name);

        public static Graph<Port> AreEqual(Port p1, Port p2)
            => env =>
            {
                var areEqual = new AreEqualConstraint();
                return (
                    env
                        .Bind(p1, areEqual.Left)
                        .Bind(p2, areEqual.Right),
                    areEqual.Result
                );
            };
        
        public static Graph<Port> GreaterThan(Port p1, Port p2)
            => env =>
            {
                var greaterThan = new GreaterThanConstraint();
                return (
                    env
                        .Bind(p1, greaterThan.Left)
                        .Bind(p2, greaterThan.Right),
                    greaterThan.Result
                );
            };

        public static Graph<T> Pin<T>(Port p, T value)
            => env => (
                env.Pin(p, new BoolDomain()),
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

        public static DisjunctGraph<Domain> Domain(Port port)
            => new DisjunctGraph<Domain>(env => env.GetDomain(port));
        
        public static Graph<Domain> Set(Port port, Domain domain)
            => throw new NotImplementedException();

        public static Graph<IEnumerable<Domain>> SetMany(Domain domain, params Port[] ports)
            => from port in ports
               from d in Set(port, domain)
               select d;

        public static Graph<Domain> SetNever(params Port[] ports)
            => from _ in SetMany(Domains.Never, ports)
               select Domains.Never;
    }


    public static class DomainOps
    {
        public static Domain Invert(Domain domain)
            => throw new NotImplementedException();
    }
    

    public static class GraphExtensions
    {
        public static Graph<TResult> Select<TSource, TResult>(this Graph<TSource> source, Func<TSource, TResult> select)
            => env =>
            {
                var (env2, v) = source(env);
                return (env2, select(v));
            };

        public static Graph<TTo> SelectMany<TFrom, TVia, TTo>(this Graph<TFrom> source, Func<TFrom, Graph<TVia>> collectionSelector, Func<TFrom, TVia, TTo> resultSelector)
            => env =>
            {
                var (env2, v1) = source(env);
                var (env3, v2) = collectionSelector(v1)(env2);
                return (env3, resultSelector(v1, v2));
            };
        
        public static Graph<IEnumerable<TTo>> SelectMany<TFrom, TVia, TTo>(this IEnumerable<TFrom> source, Func<TFrom, Graph<TVia>> collectionSelector, Func<TFrom, TVia, TTo> resultSelector)
            => source.Aggregate(
                Graph.Lift(Enumerable.Empty<TTo>()),
                (gAcc, @from) =>
                    from acc in gAcc
                    from via in collectionSelector(@from)
                    let to = resultSelector(@from, via)
                    select acc.Concat(new[] { to }));

        public static DisjunctGraph<TTo> SelectMany<TFrom, TVia, TTo>(this DisjunctGraph<TFrom> source, Func<TFrom, DisjunctGraph<TVia>> collectionSelector, Func<TFrom, TVia, TTo> resultSelector)
            => new DisjunctGraph<TTo>(env =>
            {
                var (env2, outers) = source.Invoke(env);

                return outers.Aggregate(
                    (env2, Enumerable.Empty<(Env, TTo)>()),
                    (outerAc, outer) =>
                    {
                        var (acEnv, acTups) = outerAc;
                        var (env3, @from) = outer;
                        
                        var (env4, inners) = collectionSelector(@from).Invoke(env3);
                        
                        var (env5, innerTos) = inners.Aggregate(
                            (env4, Enumerable.Empty<(Env, TTo)>()),
                            (innerAc, inner) =>
                            {
                                var (innerAcEnv, innerAcTups) = innerAc;
                                var (innerEnv, via) = inner;
                                return (
                                    Env.Merge(innerAcEnv, innerEnv),
                                    innerAcTups.Concat(new[] { (innerEnv, resultSelector(@from, via)) })
                                    );
                            });
                        
                        return (
                            Env.Merge(acEnv, env5), //only merge envs for benefit of root(?) - evaluation of disjunctions should be isolated
                            acTups.Concat(innerTos)
                            );
                    });
            });

        public static DisjunctGraph<TTo> SelectMany<TFrom, TVia, TTo>(this Graph<TFrom> source, Func<TFrom, DisjunctGraph<TVia>> collectionSelector, Func<TFrom, TVia, TTo> resultSelector)
            => ((DisjunctGraph<TFrom>) source).SelectMany(collectionSelector, resultSelector);
        
        public static DisjunctGraph<TTo> SelectMany<TFrom, TVia, TTo>(this DisjunctGraph<TFrom> source, Func<TFrom, Graph<TVia>> collectionSelector, Func<TFrom, TVia, TTo> resultSelector)
            => source.SelectMany(
                from => (DisjunctGraph<TVia>)collectionSelector(from),
                resultSelector);
    }
}