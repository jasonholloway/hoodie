namespace Hoodie.GroupMaps
{
    public static class GroupMapExtensions
    {
        public static Map<N, V> Combine<N, V>(this Map<N, V> left, Map<N, V> right, IMonoid<V> monoidV)
        {
            var monoid = new GroupMapMonoid<N, V>(new GroupMonoid<N, V>(monoidV));
            return monoid.Combine(left, right);
        }
        
        public static Map<N, Sym> Combine<N>(this Map<N, Sym> left, Map<N, Sym> right)
        {
            var monoid = new GroupMapMonoid<N, Sym>(new GroupMonoid<N, Sym>(new SymMonoid()));
            return monoid.Combine(left, right);
        }
    }
}