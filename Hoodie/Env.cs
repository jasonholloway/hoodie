using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Hoodie
{
    public class Env
    {
        private readonly ImmutableDictionary<string, Var> _vars;
        private readonly ImmutableDictionary<Port, (Env, Binding)[]> _binds;

        private Env(ImmutableDictionary<string, Var> vars = default, ImmutableDictionary<Port, Binding> binds = default)
        {
            _vars = vars;
            _binds = binds;
        }
        
        public Env() 
            : this(ImmutableDictionary<string, Var>.Empty, ImmutableDictionary<Port, Binding>.Empty)
        { }
        
        public IEnumerable<(Env, Binding)> SummonBinds(Port port)
            => _binds.TryGetValue(port, out var binding)
                ? binding
                : new Binding(port);

        public (Env, Binding) PutBind(Binding binding)
        {
            var newBinds = _binds.SetItems(
                binding.Ports.Select(p => 
                    new KeyValuePair<Port, Binding>(p, binding)));
            
            var env = new Env(_vars, newBinds);
            return (env, binding);
        }

        public (Env, Var) SummonVar(string name)
        {
            if (_vars.TryGetValue(name, out var found))
            {
                return (this, found);
            }

            var @var = new Var(name);
            var env = new Env(_vars.SetItem(name, @var), _binds);
            return (env, @var);
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