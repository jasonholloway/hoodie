using System.Collections.Generic;
using System.Linq;

namespace Hoodie
{
    public static class BaseOps
    {
        public static Graph<Domain> Bind(IEnumerable<Bindable> bindables) =>
            from preBinds in
                from bindable in bindables
                from binding in bindable.Inner switch
                {
                    Domain domain => Graph.Lift(new Binding(domain)),
                    Port port => SummonBinding(port),
                }
                select binding
                
            from bind1 in PutBinding(
                preBinds.Aggregate(Binding.Empty, Binding.Merge))

            from _ in
                from port in bind1.Ports
                from bind2 in SummonBinding(port)
                from domain2 in Propagate(port, bind2) //but after each leg, we should update the binding
                from _ in PutBinding(bind2.AddDomain(domain2))
                select true

            select bind1.Domain;
        
        //
        //could be cleaner here it feels, without so many puttings and summonings...
        //
        //but also, what of disjunctions? what we get back from a propagation via a port isn't just one answer necessarily, it can be multiple
        //so this entire propagation logic seems to be off
        //in fact, bindings don't just have single domains, do they? well - they do, but t
        //
        //
        
        //so we propagate into one port thinking we have one single domain
        //but the answer that comes back is that there are two possible worlds
        //these possible worlds should then be encoded in the binding, no?
        //
        //when we are at the root level, then we know we have a port, and we want to know its disjunct domain: this or this or this...
        //so a binding doesn't just have a domain, or...
        //
        //a binding is a window into other worlds, other envs
        //which is all great except a binding is in the first instance in an env
        //but that is just the means of finding the binding in the first place; from here on we're travelling into other envs at each step
        //
        //long story short, a binding must have an enumeration of (Env, Domain) tuples
        //in the env, the domain is bound to the port
        //
        //
        //
        
        private static DisjunctGraph<Domain> Propagate(Port port, Binding bind1) =>
            from replyDomain in port.Update(bind1.Domain)
            
            from domain2 in replyDomain.Equals(bind1.Domain)
                ? Graph.Lift(replyDomain)
                : (from bind2 in PutBinding(bind1.AddDomain(replyDomain))
                   from otherDomains in 
                       from otherPort in bind2.Ports.Except(new[] {port})
                       from domain2 in Propagate(otherPort, bind2) 
                       select domain2
                   select otherDomains.Aggregate(Domains.Any, (ac, d) => ac))

            select domain2;

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
        
        private static Graph<Binding> SummonBinding(Port port)
            => env => (env, env.SummonBinding(port));
        
        private static Graph<Binding> PutBinding(Binding binding)
            => env => env.PutBinding(binding);
    }
}