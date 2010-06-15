namespace Truesight.Decompiler.Domains
{
    public partial class Domain
    {
        private static Domain _current = new Domain();
        public static Domain Current
        {
            get { return _current; }
            set { _current = value ?? new Domain(); }
        }
    }
}
