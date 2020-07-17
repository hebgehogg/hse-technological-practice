namespace WebApiInfSyst.DBwablon
{
    public class StoreForm
    {
        private readonly string _type;
        private readonly string _name;
        private readonly string _order;
        private readonly string[] _genre;
        public StoreForm() { }
        public StoreForm(string type, string name, string order, string[] genre)
        {
            _type = type;
            _name = name;
            _order = order;
            _genre = genre;
        }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Order { get; set; }
        public string[] Genre { get; set; }
        public bool ff { get; set; }
    }
}
