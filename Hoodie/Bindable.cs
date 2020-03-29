namespace Hoodie
{
    public class Bindable
    {
        public object Inner { get; }

        private Bindable(object inner)
        {
            Inner = inner;
        }
        
        public static implicit operator Bindable(Domain domain)
            => new Bindable(domain);
        
        public static implicit operator Bindable(Port port)
            => new Bindable(port);
        
        public static implicit operator Bindable(Var @var)
            => new Bindable(@var.Port);
        
        public static implicit operator Bindable(bool val)
            => new Bindable(val ? (Domain)new TrueDomain() : new FalseDomain());
        
        public static implicit operator Bindable(int val)
            => new Bindable(new IntDomain());
    }
}