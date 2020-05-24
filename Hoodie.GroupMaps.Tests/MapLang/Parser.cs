using System.Collections.Generic;

namespace Hoodie.GroupMaps.Tests.MapLang
{
    public static class Parser
    {
        public static Node Parse(IEnumerable<string[]> slices)
        {
            var x = new Context(slices);

            Read();

            if (x.TryPop(out Node result))
                return result;
            else
                return null;

            bool Read()
            {
                if (ReadGrid())
                {
                    ReadForwards();
                    return true;
                }

                return false;
            }

            bool ReadForwards()
            {
                if (ReadDisjunction()
                    || ReadCombination()
                    || ReadHitEquals()
                    || ReadEquals())
                {
                    ReadForwards();
                    return true;
                }

                return false;
            }
            
            bool ReadGrid()
            {
                if (x.TryRead(matchAll: @"^\w*$", out var head))
                {
                    if (ReadGrid() 
                        && x.TryPop(out GridNode tail))
                    {
                        x.Push(new GridNode(head, tail));
                        return true;
                    }
                    else
                    {
                        x.Push(new GridNode(head, null));
                        return true;
                    }
                }

                return false;
            }

            bool ReadCombination()
            {
                x.Save();
                
                if(x.TryPop(out GridNode left)
                    && x.ReadSymbol("*")
                    && Read()
                    && x.TryPop(out GridNode right))
                {
                    x.Push(new CombinationNode(left, right));
                    return true;
                }

                x.Reset();
                return false;
            }

            bool ReadEquals()
            {
                x.Save();

                if (
                    x.TryPop(out GridNode left)
                    && x.ReadSymbol("=")
                    && Read()
                    && x.TryPop(out GridNode right))
                {
                    x.Push(new EqualsNode(left, right));
                    return true;
                }

                x.Reset();
                return false;
            }
            
            bool ReadHitEquals()
            {
                x.Save();
                
                if(x.TryPop(out GridNode map)
                   && ReadHitOp(out var nodes)
                   && Read()
                   && x.TryPop(out DisjunctionNode expected))
                {
                    x.Push(new HitNode(map, nodes, expected));
                    return true;
                }
                
                x.Reset();
                return false;
            }

            bool ReadHitOp(out ISet<int> nodes)
                => x.ReadPosSymbol("=>", out nodes);

            bool ReadDisjunction()
            {
                x.Save();

                if(x.TryPop(out GridNode left) 
                    && x.ReadSymbol("^")
                    && Read()
                    && x.TryPop(out Node right))
                {
                    x.Push(new DisjunctionNode(left, right));
                    return true;
                }
                
                x.Reset();
                return false;
            }
        }

    }
}