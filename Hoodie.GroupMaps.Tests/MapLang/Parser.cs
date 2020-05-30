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
                    || ReadCropEquals()
                    || ReadHitEquals()
                    || ReadInequality()
                    || ReadEquality())
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
                
                if(x.TryPop(out Node left)
                    && x.ReadSymbol("*")
                    && Read()
                    && x.TryPop(out Node right))
                {
                    x.Push(new CombinationNode(left, right));
                    return true;
                }

                x.Reset();
                return false;
            }

            bool ReadEquality()
            {
                x.Save();

                if (x.TryPop(out Node left)
                    && x.ReadSymbol("=")
                    && Read()
                    && x.TryPop(out Node right))
                {
                    x.Push(new EqualsNode(left, right));
                    return true;
                }

                x.Reset();
                return false;
            }
            
            bool ReadInequality()
            {
                x.Save();

                if (x.TryPop(out Node left)
                    && x.ReadSymbol("!=")
                    && Read()
                    && x.TryPop(out Node right))
                {
                    x.Push(new InequalsNode(left, right));
                    return true;
                }

                x.Reset();
                return false;
            }
            
            bool ReadCropEquals()
            {
                x.Save();
                
                if(x.TryPop(out Node map)
                   && x.ReadPosSymbol("#>", out var nodes)
                   && Read()
                   && x.TryPop(out Node expected))
                {
                    x.Push(new CropNode(map, nodes, expected));
                    return true;
                }
                
                x.Reset();
                return false;
            }
            
            bool ReadHitEquals()
            {
                x.Save();
                
                if(x.TryPop(out Node map)
                   && x.ReadPosSymbol("=>", out var nodes)
                   && Read()
                   && x.TryPop(out Node expected))
                {
                    x.Push(new HitNode(map, nodes, expected));
                    return true;
                }
                
                x.Reset();
                return false;
            }

            bool ReadDisjunction()
            {
                x.Save();

                if(x.TryPop(out Node left) 
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