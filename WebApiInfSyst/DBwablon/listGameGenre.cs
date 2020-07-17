namespace WebApiInfSyst.DBwablon
{
    public class listGameGenre
    {
        private readonly string _gname;
        private readonly string _genre;
        public listGameGenre(string gname, string genre)
        {
            _gname = gname;
            _genre = genre;
        }
        public string GameName { get { return _gname; } }
        public string Genre { get { return _genre; } }
    }
}
