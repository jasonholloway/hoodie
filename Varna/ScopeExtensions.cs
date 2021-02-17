namespace Varna
{
    public static class ScopeExtensions
    {
        public static Scope Complete(this Scope x)
            => x.More?.Invoke().Complete() ?? x;
    }
}