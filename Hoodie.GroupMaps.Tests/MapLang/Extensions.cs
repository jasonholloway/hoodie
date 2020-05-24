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
    }
}