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
        public static T Run<T>(string code)
            => (T)Run(code);
        
        public static object Run(string code)
        {
            var slices = ParseSlices(code);
            var x = new Context(slices);

            while (!x.AtEnd) Read();

            if (x.TryPop(out object result))
                return result;
            else
                return null;

            bool Read()
                => ReadMap()
                   || ReadClumps()
                   || ReadCombination()
                   || ReadHitEquals()
                   || ReadEquals()
                   || Throw("Can't read input!");
            
            bool Throw(string message)
                => throw new Exception(message);
            
            bool ReadCombination()
            {
                x.Save();
                
                if(x.TryPop(out Map<int, Sym> left)
                    && x.ReadSymbol("*")
                    && Read()
                    && x.TryPop(out Map<int, Sym> right))
                {
                    var map = left.Combine(right);
                    x.Push(map);
                    return true;
                }

                x.Reset();
                return false;
            }

            bool ReadEquals()
            {
                x.Save();

                if (
                    x.TryPop(out Map<int, Sym> left)
                    && x.ReadSymbol("=")
                    && Read()
                    && x.TryPop(out Map<int, Sym> right))
                {
                    Assert.That(left, Is.EqualTo(right).Using(MapComp));
                    return true;
                }

                x.Reset();
                return false;
            }
            
            bool ReadHitEquals()
            {
                x.Save();
                
                if(x.TryPop(out Map<int, Sym> map)
                   && ReadHitOp(out var nodes)
                   && Read()
                   && x.TryPop(out ISet<Map<int, Sym>> expected))
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


            bool ReadMap()
            {
                if (ReadGroups(out var groups))
                {
                    var map = x.TryPop<Map<int, Sym>>(out var m) 
                        ? m : Map<int, Sym>.Empty;
                    
                    foreach (var g in groups)
                    {
                        map = map.Add(GroupMap.Lift(g.Nodes, g.Value));
                    }
                    
                    x.Push(map);

                    ReadMap();
                    
                    return true;
                }

                return false;
            }

            bool ReadGroups(out SimpleGroup<int, Sym>[] found)
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
            
            bool ReadClumps()
            {
                x.Save();

                ISet<Map<int, Sym>> acc;

                if (x.TryPop(out Map<int, Sym> left))
                {
                    acc = new[] {left}.ToHashSet();
                }
                else if (x.TryPop(out ISet<Map<int, Sym>> _acc))
                {
                    acc = _acc;
                }
                else
                {
                    x.Reset();
                    return false;
                }
                
                if(x.ReadSymbol("^")
                    && ReadMap()
                    && x.TryPop(out Map<int, Sym> right))
                {
                    acc.Add(right);
                    x.Push(right);
                    return true;
                }
                
                x.Reset();
                return false;
            }

            IEnumerable<string[]> ParseSlices(string _input)
            {
                var matches = Regex
                    .Matches(_input, @"^(?: +([\w\.\|_\#\^\<\>\=\*]+))+", RegexOptions.Multiline);

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
            Stack<(ImmutableList<string[]>, ImmutableStack<object>)> _saved;
            
            ImmutableList<string[]> _slices;
            ImmutableStack<object> _stack;

            public Context(IEnumerable<string[]> slices)
            {
                _saved = new Stack<(ImmutableList<string[]>, ImmutableStack<object>)>();
                _slices = ImmutableList.CreateRange(slices);
                _stack = ImmutableStack<object>.Empty;
            }

            public bool AtEnd => _slices.IsEmpty;
            
            public void Save()
            {
                _saved.Push((_slices, _stack));
            }

            public void Reset()
            {
                (_slices, _stack) = _saved.Pop();
            }

            public void Push(object obj)
            {
                _stack = _stack.Push(obj);
            }

            public bool TryPop<T>(out T val)
            {
                if (!_stack.IsEmpty && _stack.Peek() is T)
                {
                    val = (T)_stack.Peek();
                    _stack = _stack.Pop();
                    return true;
                }

                val = default;
                return false;
            }

            public bool TryRead(out string[] slice)
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
                => TryRead(out slice)
                   && slice.All(r => Regex.IsMatch(r, matchAll))
                   && Move();

            public bool Move()
            {
                if (!_slices.IsEmpty)
                {
                    _slices = _slices.RemoveAt(0);
                    return true;
                }

                return false;
            }

            public bool TryReadSymbol(string symbol)
                => TryRead(out var slice)
                   && slice.Where(r => r != "").Any(r => r == symbol);
            
            public bool ReadSymbol(string symbol)
                => TryReadSymbol(symbol)
                   && Move();

            public bool ReadPosSymbol(string symbol, out ISet<int> nodes)
            {
                if (TryReadSymbol(symbol) && TryRead(out var slice))
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