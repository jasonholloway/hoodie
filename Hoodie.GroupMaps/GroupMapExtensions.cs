using System.Collections.Generic;

namespace Hoodie.GroupMaps
{
    public static class GroupMapExtensions
    {
        public static GroupMap<N, V> Combine<N, V>(this GroupMap<N, V> left, GroupMap<N, V> right, IMonoid<V> monoidV)
        {
            var monoid = new GroupMapMonoid<N, V>(new GroupMonoid<N, V>(monoidV));
            return monoid.Combine(left, right);
        }
        
        public static GroupMap<N, string> Combine<N>(this GroupMap<N, string> left, GroupMap<N, string> right)
        {
            var monoid = new GroupMapMonoid<N, string>(new GroupMonoid<N, string>(new StringMonoid()));
            return monoid.Combine(left, right);
        }


        // public static GroupMap<N, V> JoinGroups<N, V>(this GroupMap<N, V> map, IEnumerable<Group<N, V>> leftGroups)
        // {
        //     
        // }
        
        
    }
}