using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hoodie.GroupMaps.Tests.MapLang
{
    internal class Context
    {
        Stack<(ImmutableList<string[]>, ImmutableStack<Node>)> _saved;
            
        ImmutableList<string[]> _slices;
        ImmutableStack<Node> _stack;

        public Context(IEnumerable<string[]> slices)
        {
            _saved = new Stack<(ImmutableList<string[]>, ImmutableStack<Node>)>();
            _slices = ImmutableList.CreateRange(slices);
            _stack = ImmutableStack<Node>.Empty;
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

        public void Push(Node node)
        {
            _stack = _stack.Push(node);
        }

        public bool TryPop<T>(out T val)
            where T : Node
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

                return Move();
            }

            nodes = null;
            return false;
        }
    }
}