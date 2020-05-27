using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Hoodie.GroupMaps.Tests.MapLang
{
    public static class Extensions
    {
        public static T As<T>(this object val)
        {
            if (val is T typedVal) return typedVal;
            else throw new Exception($"Expected {val} to have type {typeof(T)}!");
        }
        
        public static ISet<T> AsSet<T>(this object val)
            => val switch
            {
                T typedVal => ImmutableHashSet<T>.Empty.Add(typedVal),
                ISet<T> setVal => setVal,
                _ => throw new Exception($"Expected {val} to have type {typeof(T)}!")
            };
        
        public static Disjunction<int, Sym> AsDisjunction(this object val)
            => val switch
            {
                Disjunction<int, Sym> d => d,
                ISet<Map<int, Sym>> set => new Disjunction<int, Sym>(set),
                Map<int, Sym> map => new Disjunction<int, Sym>(new[] { map }),
                _ => throw new Exception($"Bad value {val}")
            };
    }
}