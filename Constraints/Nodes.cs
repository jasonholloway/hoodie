namespace probs
{
    public abstract class Node
    {
        public static Node operator ==(Node me, object val)
            => new EqualsNode(me, new ConstantNode(val));

        public static Node operator !=(Node me, object val) 
            => new NotNode(new EqualsNode(me, new ConstantNode(val)));
        
        public static Node operator &(Node me, Node other) => default;
        public static Node operator ^(Node me, Node other) => default;
    }
    
    public class NotNode : Node
    {
        public Node Node { get; }

        public NotNode(Node node)
        {
            Node = node;
        }
    }

    public class EqualsNode : Node
    {
        public Node Left { get; }
        public Node Right { get; }

        public EqualsNode(Node left, Node right)
        {
            Left = left;
            Right = right;
        }
    }
    
    public class VariableNode : Node
    {
        public Variable Variable { get; }

        public VariableNode(Variable variable)
        {
            Variable = variable;
        }
    }

    public class ConstantNode : Node
    {
        public object Value { get; }

        public ConstantNode(object value)
        {
            Value = value;
        }
    }
}