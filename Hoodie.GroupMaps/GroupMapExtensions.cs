namespace Hoodie.GroupMaps
{
    public static class GroupMapExtensions
    {
        public static Map<N, Sym> Combine<N>(this Map<N, Sym> left, Map<N, Sym> right) 
            => left.Combine(right, new SymMonoid());
    }
}