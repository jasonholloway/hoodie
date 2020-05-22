using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Hoodie.GroupMaps.Tests
{
    public static class MapLang
    {
        public static void Test(string code)
        {
            var slices = ParseSlices(code);
            var x = new Context(slices);

            while (ReadEquals() 
                   || ReadHit());

            return;

            bool ReadEquals()
            {
                x.Save();

                if (ReadMap(out var left)
                    && x.ReadSymbol("=")
                    && ReadMap(out var right))
                {
                    Assert.That(right, Is.EqualTo(left).Using(MapComp));
                    return true;
                }

                x.Reset();
                return false;
            }
            
            bool ReadHit()
            {
                x.Save();
                
                if (ReadMap(out var map)
                   && ReadHitOp(out var nodes)
                   && ReadClumps(out var expected))
                {
                    var result = map.Hit(nodes);
                    Assert.That(result, Is.EqualTo(expected));
                    return true;
                }
                
                x.Reset();
                return false;
            }

            bool ReadHitOp(out ISet<int> nodes)
                => x.ReadPosSymbol("=>", out nodes);
            

            bool ReadMap(out Map<int, Sym> map)
            {
                map = Map<int, Sym>.Empty;
                
                while (ReadGroup(out var found))
                {
                    foreach (var g in found)
                    {
                        map = map.Add(GroupMap.Lift(g.Nodes, g.Value));
                    }
                }

                return true;
            }

            bool ReadGroup(out SimpleGroup<int, Sym>[] found)
            {
                if (x.TryRead(matchAll: @"^\w*$", out var slice))
                {
                    found = slice
                        .Select((v, y) => (y: y + 1, v))
                        .Where(t => t.v != "")
                        .GroupBy(t => t.v)
                        .Select(g => SimpleGroup.From(g.Select(t => t.y), Sym.From(g.Key)))
                        .ToArray();
                    
                    return true;
                }
                
                found = null;
                return false;
            }
            
            
            bool ReadClumps(out ISet<Map<int, Sym>> clumps)
            {
                var found = new Queue<Map<int, Sym>>();
                
                do
                {
                    if (!ReadMap(out var clump)) break;
                    else
                    {
                        found.Enqueue(clump);
                    }
                } while (x.ReadSymbol("^"));

                clumps = found.ToHashSet();
                return true;
            }

            IEnumerable<string[]> ParseSlices(string _input)
            {
                var matches = Regex
                    .Matches(_input, @"^(?: +([\w\.\|_\#\^\<\>\=]+))+", RegexOptions.Multiline);

                return new Queue<string[]>(matches
                    .SelectMany((m, y) => m.Groups[1].Captures.Select((c, x) => (x, y, c.Value)))
                    .GroupBy(t => t.x, t => t.Value)
                    .Select(g => g
                        .Select(r => Regex.Replace(r, @"[_\.\|]", ""))
                        .ToArray()));
            }
        }

        class Context
        {
            Stack<ImmutableList<string[]>> _saved;
            ImmutableList<string[]> _slices;

            public Context(IEnumerable<string[]> slices)
            {
                _saved = new Stack<ImmutableList<string[]>>();
                _slices = ImmutableList.CreateRange(slices);
            }

            public void Save()
            {
                _saved.Push(_slices);
            }

            public void Reset()
            {
                _slices = _saved.Pop();
            }

            public bool TryPeek(out string[] slice)
            {
                if (!_slices.IsEmpty)
                {
                    slice = _slices[0];
                    return true;
                }

                slice = null;
                return false;
            }

            public bool TryRead(string matchAll, out string[] slice)
                => TryPeek(out slice)
                   && slice.All(r => Regex.IsMatch(r, matchAll))
                   && Read();

            public bool Read()
            {
                if (!_slices.IsEmpty)
                {
                    _slices = _slices.RemoveAt(0);
                    return true;
                }

                return false;
            }

            public bool TryReadSymbol(string symbol)
                => TryPeek(out var slice)
                   && slice.Where(r => r != "").Any(r => r == symbol);
            
            public bool ReadSymbol(string symbol)
                => TryReadSymbol(symbol)
                   && Read();

            public bool ReadPosSymbol(string symbol, out ISet<int> nodes)
            {
                if (TryReadSymbol(symbol) && TryPeek(out var slice))
                {
                    nodes = slice
                        .Select((v, y) => (y: y + 1, v))
                        .Where(t => t.v != "")
                        .Select(t => t.y)
                        .ToHashSet();

                    return true;
                }

                nodes = null;
                return false;
            }
        }
        
        internal static readonly Comparison<Map<int, Sym>> MapComp = (m1, m2) =>
        {
            var groups1 = m1.Groups.Select(g => g.Simple()).ToHashSet();
            var groups2 = m2.Groups.Select(g => g.Simple()).ToHashSet();
            return groups1.SetEquals(groups2) ? 0 : -1;
        };
    }
}