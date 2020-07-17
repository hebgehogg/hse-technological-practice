namespace WebApiInfSyst.DBwablon
{
    public class BuyForm
    {
        private readonly string _giw;
        private readonly int _cash;
        private readonly int _act;
        private readonly string _name;
        private readonly bool _error;
        public BuyForm(string giw, int cash, int act, string name, bool error)
        {
            _giw = giw;
            _cash = cash;
            _act = act;
            _name = name;
            _error = error;
        }
        public string GIW { get { return _giw; } }
        public int Cash { get { return _cash; } }
        public int ACT { get { return _act; } }
        public string Name { get { return _name; } }
        public bool Error { get { return _error; } }
    }
}
