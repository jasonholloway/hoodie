using System.Linq;

namespace Hoodie.GroupMaps.Tests
{
    public static class Helpers 
    {
        public static Map<int, Sym> EmptyMap = Map<int, Sym>.Empty;
        
        public static Map<int, Sym> Map(params (int[], Sym)[] groups)
            => groups.Aggregate(
                EmptyMap,
                (m, t) => m.Add(GroupMap.Lift(t.Item1, t.Item2)));

        public static Map<int, Sym> Map(Sym sym)
            => Map((new int[0], sym));
        
        public static Map<int, Sym> Map(int node, Sym val)
            => Map((new[] { node }, val));
        
        public static Map<int, Sym> Map((int, int) nodes, Sym val)
            => Map((new[] { nodes.Item1, nodes.Item2 }, val));
        
        public static Map<int, Sym> Map((int, int, int) nodes, Sym val)
            => Map((new[] { nodes.Item1, nodes.Item2, nodes.Item3 }, val));
        

        public static SimpleGroup<int, Sym> Group(int node, Sym sym)
            => SimpleGroup.From(new[] {node}, sym);
        
        public static SimpleGroup<int, Sym> Group((int, int) nodes, Sym sym)
            => SimpleGroup.From(new[] {nodes.Item1, nodes.Item2}, sym);
        
        public static SimpleGroup<int, Sym> Group((int, int, int) nodes, Sym sym)
            => SimpleGroup.From(new[] {nodes.Item1, nodes.Item2, nodes.Item3}, sym);
    }
}