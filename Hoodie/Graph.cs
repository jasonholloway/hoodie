using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Hoodie
{
    public class Graph
    {
        private readonly ImmutableDictionary<string, Var> _vars;
        private readonly Lookup<Port, Binding> _binds;

        private Graph(ImmutableDictionary<string, Var> vars = default, Lookup<Port, Binding> binds = default)
        {
            _vars = vars ?? ImmutableDictionary<string, Var>.Empty;
            _binds = binds ?? new Lookup<Port, Binding>();
        }
        
        public IEnumerable<Binding> GetBinds(Port port)
            => _binds[port];

        public (Graph, Binding) PutBind(Binding binding)
        {
            throw new NotImplementedException();
            // var newBinds = _binds.SetItems(
            //     binding.Ports.Select(p => 
            //         new KeyValuePair<Port, Binding>(p, binding)));
            //
            // var env = new Env(_vars, newBinds);
            // return (env, binding);
        }

        public (Graph, Var) SummonVar(string name)
        {
            if (_vars.TryGetValue(name, out var found))
            {
                return (this, found);
            }

            var @var = new Var(name);
            var env = new Graph(_vars.SetItem(name, @var), _binds);
            return (env, @var);
        }

        public (Graph, IEnumerable<(Graph, Domain)>) GetDomain(Port port)
        {
            throw new NotImplementedException();
        }

        public static Graph From(Binding binding)
        {
            
        }
        
        public static Graph Empty = new Graph(ImmutableDictionary<string, Var>.Empty, new Lookup<Port, Binding>());

        public static Graph Self = null;

        public static Graph Merge(params Graph[] envs)
        {
            throw new NotImplementedException();
        }
    }
}