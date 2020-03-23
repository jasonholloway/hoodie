using System;
using NUnit.Framework;

namespace Graff.Test
{
    using static GraphOps;
    
    
    //instead of a graph with its arcs,
    //what we have are coupled massy constraints
    //`<` has three ports, for instance
    //`==` also has three ports
    
    //these ports aren't attached by arcs, but are bound to each other
    //ports are owned by constraints or variables
    //
    
    public class Tests
    {
        [Test]
        public void Test1()
        {
            var graph =
                from node1 in SummonNode("node1")
                from node2 in SummonNode("node2")
                from _ in AddArc(node1.Out, node2.In)
                select true;
            
            var graph2 =
                from _ in graph
                from node1 in SummonNode("node1")
                select node1;

            var outp = graph2();
        }
    }
    

    public struct Arc
    {
        public readonly Port From;
        public readonly Port To;

        public Arc(Port @from, Port to)
        {
            From = @from;
            To = to;
        }
    }

    public struct Port
    {
    }

    public struct Node
    {
        public readonly Port In;
        public readonly Port Out;

        public Node(Port @in, Port @out)
        {
            In = @in;
            Out = @out;
        }
    }

    public struct Var
    {
        
    }

    public delegate TVal Graph<TVal>(GraphEnv env = default);


    public static class GraphOps
    {
        public static Graph<Node> SummonNode(string id)
        {
            throw new NotImplementedException();
        }
        
        public static Graph<Arc> AddArc(Port @from, Port @to)
        {
            throw new NotImplementedException();
        }

        public static Graph<Var> SummonVar(string name)
        {
            throw new NotImplementedException();
            
        }
    }

    public struct GraphEnv
    {
        public static Graph<Node> GetNode(string id)
        {
            throw new NotImplementedException();
        }
        
        public static Graph<Node> AddNode()
        {
            throw new System.NotImplementedException();
        }
    }
        // public static IEnumerable<TResult> Select<TSource, TResult>(
        //     this IEnumerable<TSource> source, Func<TSource, TResult> selector)

    public static class GraphExtensions
    {
        public static Graph<TResult> Select<TSource, TResult>(this Graph<TSource> context, Func<TSource, TResult> select)
        {
            throw new NotImplementedException();
        }
        
        public static Graph<TTo> SelectMany<TFrom, TVia, TTo>(this Graph<TFrom> graph, Func<TFrom, Graph<TVia>> collectionSelector, Func<TFrom, TVia, TTo> resultSelector)
        {
            throw new NotImplementedException();
        }
        
        public static Graph<TTo> SelectMany<TFrom, TTo>(this Graph<TFrom> graph, Func<TFrom, Graph<TTo>> collectionSelector)
        {
            throw new NotImplementedException();
        }
        
    }


}