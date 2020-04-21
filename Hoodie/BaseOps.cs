using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Hoodie
{
    public static class BaseOps
    {
        // private static IEnumerable<(Env, Binding)> MergeBinds(IEnumerable<(Env, Binding)> binds)
        //     => binds.Distinct().Aggregate(Binding.Empty, Binding.Merge);

            //below should prob just be graph op
        public static GraphOp<object> Bind(IEnumerable<Bindable> bindables) =>
            graph1 =>
            {
                var domain = bindables
                    .Select(b => b.Inner).OfType<Domain>()
                    .Aggregate(Domains.Any, Domain.Multiply);

                var ports = bindables
                    .Select(b => b.Inner).OfType<Port>()
                    .ToImmutableHashSet();

                var bind = new Binding(ports, new[] {(domain, Graph.Self)}.ToImmutableArray());

                var graph2 = Graph.From(bind);
                
                //but each addition of ports and domains should be done piecemeal
                //as we do the initial bind, we might have constants in play for instance
                //instead of chucking all the bits in a bag and then saying 'merge' (tho this would be most efficient)
                //
                //well, this is true: it makes sense for there to be control over how everything gets merged, how the merging is modularised
                //smaller local merges = faster of course
                //
                //it makes sense for us to piece together our new graph bit by bit, and in the order that the bindables have been specified
                //(always domains first though, but afterwards, the ordering of the ports matters)
                //
                //so we are here, despite the expectation of simplicity from `Bind`, doing something composite, secondary
                //what would be primary? the merging of one new element: even the simplest merge will cause propagation
                
                //SERIES OF PIECEMEAL MERGES PLEASE
                //THE PRIMARY OP IS GRAPH.MERGE - this does everything, inc propagation

                var merged = Graph.Merge(graph1, graph2);
                
                return (merged, default);
            };

        public static GraphOp<Graph> Merge(Graph other) =>
            env =>
            {
                return (env, env);
            };
            
        
            // from mergables in
            //     (
            //         from bindable in bindables
            //         from binding in bindable.Inner switch
            //         {
            //             Domain domain => Graph.Lift(new Binding(domain, Env.Empty)),
            //             Port port => SummonBinds(port),
            //         }
            //         select binding
            //     ).Invoke
            // from binds in PutBinds(MergeBinds(mergables))
            // from _ in Propagate(binds) //do we have to put again?
            // select Domains.Any;

        //Propagates a binding through it's ports, reintegrates into the (relative) root
        private static GraphOp<IEnumerable<(Graph, Binding)>> Propagate(IEnumerable<(Graph Env, Binding Bind)> binds) =>
            throw new NotImplementedException();
            // from x in binds 
            // //as soon as we have iterated the envBind, we should be sequencing using the x.Env
            // //this sounds about right
            // //this gets us back to the idea of 
            //
            // from port in x.Bind.Ports
            // from newBinds in Graph.From(_ =>
            // {
            //     var w = PropagatePort(x.Bind, port).Invoke(x.Env);
            //     
            //     throw new NotImplementedException();
            // })
            // from newBind in newBinds
            // select newBind;

        //Propagates a port, replied disjunctions are immediately adopted, all other ports are given chance to themselves propagate
        //as long as something has changed; if nothing has changed, then we should stop this recursive ruffling
        private static DisjunctGraph<Binding> PropagatePort(Binding bind, Port port) =>
            throw new NotImplementedException();
            // from domain in new DisjunctGraph<Disjunct>(_ =>
            // {
            //     var inner =
            //         from __ in PutBinding(disjunct.Binding.WithDomain(disjunct.Domain))
            //         from newDisjuncts in port.Propagate(disjunct.Domain).Invoke
            //         from newDisjunct in newDisjuncts
            //         select new Disjunct(newDisjunct.Item1, default, newDisjunct.Item2);
            //
            //     var q = inner.Invoke(disjunct.Env);
            // })
            // select domain;
        
        //again, do we need to save the binding domain up front? 
        //only if we're liable to get loops in this
        //the propagation might come back to the port
        //but how would it find the same env again?
        //
        //because it might be circular, without any saving disjunctions
        //but as the graph is built, the envs will be different, yes
        
        
        
            
            // env =>
            // {
            //     var fn =
            //         from domain in port.Propagate(disjunct.Domain)   //at this point, has domain changed? if not, can skip below
            //         from domain2 in domain.Equals(disjunct.Domain)
            //             ? Graph.Lift(domain)
            //             : from blah in 
            //                 from otherPort in allPorts.Except(new[] { port })
            //                 from _ in PropagatePort(disjunct, allPorts, otherPort)
            //                 from any in Graph.Lift(Domains.Any)
            //                 select any;
            //
            //     var (innerEnv, innerDisjuncts) = fn.Invoke(disjunct.Env); 
                
                //but we're performing the actual propagation in the foreign env
                //the secondary mergin is done in our current env
                //
                //
                //
                
                
                
                //offering a new domain to a disjunct: can it return a new disjunct domain? presumably yes
                //but above we are in the category of disjunction already, so we're coping with it
                //
                //so the propagation emits some disjunctions for us
                //
                //and with our new DisjunctGraph<Domain> 
                //we must take on its emitted, narrower domain as our own
                //if this has changed, then we should be telling other ports about it too
                //
                //

            //     return (env, new Disjunct[0]);
            // };
            

        // private static Graph<Domain> Propagate(Port port, Binding bind1) =>
        //     from replyDomain in port.Update(bind1.Domain)
        //     
        //     from domain2 in replyDomain.Equals(bind1.Domain)
        //         ? Graph.Lift(replyDomain)
        //         : (from bind2 in PutBinding(bind1.AddDomain(replyDomain))
        //            from otherDomains in 
        //                from otherPort in bind2.Ports.Except(new[] {port})
        //                from domain2 in Propagate(otherPort, bind2) 
        //                select domain2
        //            select otherDomains.Aggregate(Domains.Any, (ac, d) => ac))
        //
        //     select domain2;
        
        
        // private static DisjunctGraph<Binding> SummonBinds(Port port)
        //     => new DisjunctGraph<Binding>(Graph.From(
        //         env => (env, env.GetBinds(port))));
        //
        // private static Graph<IEnumerable<(Env, Binding)>> PutBinds(IEnumerable<(Env, Binding)> binds)
        //     => env => env.PutBind(binding);
    }
}