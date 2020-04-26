using System;
using System.Collections.Generic;
using System.Linq;

namespace Hoodie
{
    public delegate (Graph, TOut) GraphOp<TOut>(Graph graph);

    public static class GraphOp
    {
        public static GraphOp<TOut> Lift<TOut>(TOut val) 
            => g => (g, val);

        public static GraphOp<TOut> From<TOut>(GraphOp<TOut> graphOp)
            => graphOp;
    }
    
    
    public class DisjunctOp<T>
    {
        public readonly GraphOp<IEnumerable<(Graph, T)>> Invoke;

        public DisjunctOp(GraphOp<IEnumerable<(Graph, T)>> invoke)
        {
            Invoke = invoke;
        }

        public static implicit operator DisjunctOp<T>(GraphOp<T> graphOp)
            => new DisjunctOp<T>(env =>
            {
                var (env2, v) = graphOp(env);
                return (env2, Enumerable.Repeat((env2, v), 1));
            });
    }
    
    public static class GraphExtensions
    {
        public static GraphOp<TResult> Select<TSource, TResult>(this GraphOp<TSource> source, Func<TSource, TResult> select)
            => env =>
            {
                var (env2, v) = source(env);
                return (env2, select(v));
            };
        
        public static DisjunctOp<TResult> Select<TSource, TResult>(this DisjunctOp<TSource> source, Func<TSource, TResult> select)
            => new DisjunctOp<TResult>(env =>
            {
                var (env2, disjuncts) = source.Invoke(env);
                return (env2, disjuncts.Select(d => (d.Item1, select(d.Item2))));
            });
        
        // public static Graph<IEnumerable<TResult>> Select<TSource, TResult>(this Graph<IEnumerable<TSource>> source, Func<TSource, TResult> select)
        //     => env =>
        //     {
        //         var (env2, vals) = source(env);
        //         return (env2, vals.Select(select));
        //     };
        
        public static GraphOp<TTo> SelectMany<TFrom, TVia, TTo>(this GraphOp<TFrom> source, Func<TFrom, GraphOp<TVia>> collectionSelector, Func<TFrom, TVia, TTo> resultSelector)
            => env =>
            {
                var (env2, v1) = source(env);
                var (env3, v2) = collectionSelector(v1)(env2);
                return (env3, resultSelector(v1, v2));
            };
        
        public static GraphOp<IEnumerable<TTo>> SelectMany<TFrom, TVia, TTo>(this GraphOp<IEnumerable<TFrom>> source, Func<TFrom, GraphOp<TVia>> collectionSelector, Func<TFrom, TVia, TTo> resultSelector)
            => env =>
            {
                var (env2, froms) = source(env);
                
                return froms.Aggregate(
                    (env2, Enumerable.Empty<TTo>()),
                    (ac, @from) =>
                    {
                        var (acEnv, acTos) = ac;
                        var (viaEnv, via) = collectionSelector(@from)(acEnv);
                        var to = resultSelector(@from, via);
                        return (viaEnv, acTos.Concat(Enumerable.Repeat(to, 1)));
                    });
            };
        
        public static GraphOp<IEnumerable<TTo>> SelectMany<TFrom, TVia, TTo>(this GraphOp<IEnumerable<TFrom>> source, Func<TFrom, IEnumerable<TVia>> collectionSelector, Func<TFrom, TVia, TTo> resultSelector)
            => env =>
            {
                var (env2, froms) = source(env);
                var tos = froms.SelectMany(@from =>
                {
                    var vias = collectionSelector(@from);
                    return vias.Select(via => resultSelector(@from, via));
                });
                return (env2, tos);
            };

        public static GraphOp<IEnumerable<TTo>> SelectMany<TFrom, TVia, TTo>(this GraphOp<TFrom> source, Func<TFrom, IEnumerable<TVia>> collectionSelector, Func<TFrom, TVia, TTo> resultSelector)
            => env =>
            {
                var (env2, @from) = source(env);
                var vias = collectionSelector(@from);
                return (env2, vias.Select(via => resultSelector(@from, via)));
            };

        public static GraphOp<IEnumerable<TTo>> SelectMany<TFrom, TVia, TTo>(this IEnumerable<TFrom> source, Func<TFrom, GraphOp<TVia>> collectionSelector, Func<TFrom, TVia, TTo> resultSelector)
            => env => source.Aggregate(
                (env, Enumerable.Empty<TTo>()),
                (ac, @from) =>
                {
                    var (acEnv, acTos) = ac;
                    var (viaEnv, via) = collectionSelector(@from)(acEnv);
                    var to = resultSelector(@from, via);
                    return (viaEnv, acTos.Concat(Enumerable.Repeat(to, 1)));
                });
        
        public static DisjunctOp<TTo> SelectMany<TFrom, TVia, TTo>(this IEnumerable<TFrom> source, Func<TFrom, DisjunctOp<TVia>> collectionSelector, Func<TFrom, TVia, TTo> resultSelector)
            => throw new NotImplementedException();
        
        public static DisjunctOp<TTo> SelectMany<TFrom, TVia, TTo>(this DisjunctOp<TFrom> source, Func<TFrom, IEnumerable<TVia>> collectionSelector, Func<TFrom, TVia, TTo> resultSelector)
            => throw new NotImplementedException();

        public static DisjunctOp<TTo> SelectMany<TFrom, TVia, TTo>(this DisjunctOp<TFrom> source, Func<TFrom, DisjunctOp<TVia>> collectionSelector, Func<TFrom, TVia, TTo> resultSelector)
            => new DisjunctOp<TTo>(env =>
            {
                var (env2, outers) = source.Invoke(env);

                return outers.Aggregate(
                    (env2, Enumerable.Empty<(Graph, TTo)>()),
                    (outerAc, outer) =>
                    {
                        var (acEnv, acTups) = outerAc;
                        var (env3, @from) = outer;
                        
                        var (env4, inners) = collectionSelector(@from).Invoke(env3);
                        
                        var (env5, innerTos) = inners.Aggregate(
                            (env4, Enumerable.Empty<(Graph, TTo)>()),
                            (innerAc, inner) =>
                            {
                                var (innerAcEnv, innerAcTups) = innerAc;
                                var (innerEnv, via) = inner;
                                return (
                                    Graph.Combine(innerAcEnv, innerEnv),
                                    innerAcTups.Concat(new[] { (innerEnv, resultSelector(@from, via)) })
                                );
                            });
                        
                        return (
                            Graph.Combine(acEnv, env5), //only merge envs for benefit of root(?) - evaluation of disjunctions should be isolated
                            acTups.Concat(innerTos)
                        );
                    });
            });

        public static DisjunctOp<TTo> SelectMany<TFrom, TVia, TTo>(this GraphOp<TFrom> source, Func<TFrom, DisjunctOp<TVia>> collectionSelector, Func<TFrom, TVia, TTo> resultSelector)
            => ((DisjunctOp<TFrom>) source).SelectMany(collectionSelector, resultSelector);
        
        public static DisjunctOp<TTo> SelectMany<TFrom, TVia, TTo>(this DisjunctOp<TFrom> source, Func<TFrom, GraphOp<TVia>> collectionSelector, Func<TFrom, TVia, TTo> resultSelector)
            => source.SelectMany(
                from => (DisjunctOp<TVia>)collectionSelector(from),
                resultSelector);
    }
}