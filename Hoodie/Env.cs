using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Hoodie
{
    public class Env
    {
        private readonly ImmutableDictionary<string, Var> _vars;
        private readonly Lookup<Port, Binding> _binds;

        private Env(ImmutableDictionary<string, Var> vars = default, Lookup<Port, Binding> binds = default)
        {
            _vars = vars ?? ImmutableDictionary<string, Var>.Empty;
            _binds = binds ?? new Lookup<Port, Binding>();
        }
        
        public IEnumerable<Binding> GetBinds(Port port)
            => _binds[port];

        public (Env, Binding) PutBind(Binding binding)
        {
            throw new NotImplementedException();
            // var newBinds = _binds.SetItems(
            //     binding.Ports.Select(p => 
            //         new KeyValuePair<Port, Binding>(p, binding)));
            //
            // var env = new Env(_vars, newBinds);
            // return (env, binding);
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
        
        public static Env Empty = new Env(ImmutableDictionary<string, Var>.Empty, new Lookup<Port, Binding>());

        public static Env Merge(params Env[] envs)
        {
            throw new NotImplementedException();
        }
    }
}