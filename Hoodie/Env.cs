using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Hoodie
{
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

        public (Env, Domain) Bind(IEnumerable<Bindable> bindables)
        {
            var domains = bindables.Select(b => b.Inner).OfType<Domain>().ToArray();
            var ports = bindables.Select(b => b.Inner).OfType<Port>().ToArray();
            
            //if there are any preexisting binds on any ports, then these need to be combined
            //by default, ports are taken to be bound to 'any'
            //
            //
            
            throw new NotImplementedException();
        }

        public (Env, Var) Var(string name)
        {
            if (_vars.TryGetValue(name, out var found))
            {
                return (this, found);
            }

            var @var = new Var(name);
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
}