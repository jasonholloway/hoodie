using System;
using System.Collections.Generic;
using System.Linq;

namespace Hoodie
{
    public delegate (Env, TOut) Graph<TOut>(Env env);

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
    
    public static class GraphExtensions
    {
        public static (Env, TResult) Select<TSource, TResult>(this (Env, TSource) source, Func<TSource, TResult> select)
        {
            var (env, val) = source;
            return (env, select(val));
        }
        
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