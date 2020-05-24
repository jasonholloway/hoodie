using System.Collections.Generic;

namespace Hoodie.GroupMaps.Tests.MapLang
{
    public abstract class Node {}

    public abstract class BinaryNode<TLeft, TRight> : Node
        where TLeft : Node
        where TRight : Node
    {
        public readonly TLeft Left;
        public readonly TRight Right;

        protected BinaryNode(TLeft left, TRight right)
        {
            Left = left;
            Right = right;
        }
    }

    public class GridNode : Node
    {
        public readonly string[] Head;
        public readonly Node Tail;

        public GridNode(string[] head, Node tail)
        {
            Head = head;
            Tail = tail;
        }
    }

    public class CombinationNode : BinaryNode<Node, Node>
    {
        public CombinationNode(Node left, Node right) : base(left, right)
        { }
    }

    public class EqualsNode : BinaryNode<Node, Node>
    {
        public EqualsNode(Node left, Node right) : base(left, right)
        { }
    }

    public class DisjunctionNode : BinaryNode<Node, Node>
    {
        public DisjunctionNode(Node left, Node right) : base(left, right)
        { }
    }

    public class HitNode : BinaryNode<Node, Node>
    {
        public readonly ISet<int> Nodes;

        public HitNode(Node left, ISet<int> nodes, Node right) : base(left, right)
        {
            Nodes = nodes;
        }
    }
}