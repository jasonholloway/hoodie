namespace Hoodie
{
    public class Port
    {
        public readonly string Name;
        private readonly Ripple _ripple;

        public Port(string name, Ripple ripple)
        {
            Name = name;
            _ripple = ripple;
        }

        public override string ToString()
            => $"Port({Name})";
    }

    public delegate DisjunctGraph<Domain> Ripple(Graph<Domain> graph);
}