using System;

namespace Hoodie.GroupMaps.Tests.MapLang
{
    public static class Extensions
    {
        public static T As<T>(this object val)
        {
            if (val is T typedVal) return typedVal;
            else throw new Exception($"Expected {val} to have type {typeof(T)}!");
        }
    }
}