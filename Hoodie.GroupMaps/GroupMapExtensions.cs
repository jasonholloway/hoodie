namespace Hoodie.GroupMaps
{
    public static class GroupMapExtensions
    {
        public static GroupMap<N, V> Combine<N, V>(this GroupMap<N, V> left, GroupMap<N, V> right, IMonoid<V> monoidV)
        {
            var monoid = new GroupMapMonoid<N, V>(new GroupMonoid<N, V>(monoidV));
            return monoid.Combine(left, right);
        }
        
        public static GroupMap<N, Sym> Combine<N>(this GroupMap<N, Sym> left, GroupMap<N, Sym> right)
        {
            var monoid = new GroupMapMonoid<N, Sym>(new GroupMonoid<N, Sym>(new SymMonoid()));
            return monoid.Combine(left, right);
        }
    }
}