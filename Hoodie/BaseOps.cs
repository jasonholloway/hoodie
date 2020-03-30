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
                from replyDomain in port.Update(bind1.Domain)
                from bindDomain in UpdateBinding(port, replyDomain)
                
                from otherPort in bind1.Ports.Except(new[] { port })
                //but - the otherPort reply must be propagated to each other
                //recursive application of a submethod please!
                //
                //but - without any further checks, this back-and-forth propagation
                //will never end; there needs to be a check on propagation if nothing has changed
                //
                select true

            select bind1.Domain;
        
        
        public static Graph<Domain> UpdateBinding(Port port, Domain domain) =>
            from binding in SummonBinding(port)
            let binding2 = binding.AddDomain(domain)
            from _ in PutBinding(binding2)
            select binding2.Domain;
        
        
        public static Graph<Binding> SummonBinding(Port port)
            => env => (env, env.SummonBinding(port));
        
        public static Graph<Binding> PutBinding(Binding binding)
            => env => env.PutBinding(binding);
    }
}