namespace Hoodie
{
    public struct Disjunct<T>
    {
        public readonly Env Env;
        public readonly T Binding;

        public Domain Domain => Binding.Domain;

        public Disjunct(Env env, Binding binding)
        {
            Env = env;
            Binding = binding;
        }
    }
}