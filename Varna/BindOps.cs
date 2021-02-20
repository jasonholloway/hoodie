using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Varna
{
    static class BindOps
    {
        public static Binds InCommon(IEnumerable<Binds> bindDicts)
            => bindDicts.Aggregate((l, r) => l.Intersect(r));
        
    }
}