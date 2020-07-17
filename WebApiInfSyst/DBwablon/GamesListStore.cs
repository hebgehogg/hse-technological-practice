using System;

namespace WebApiInfSyst.DBwablon
{
    public class GamesListStore
    {
        private readonly string _gname;
        private readonly string _gcover;
        private readonly int _gprice;
        private readonly string _ggenre;
        private readonly string _dev;
        public GamesListStore(string gname, byte[] gcover,string dev, int gprice, string ggenre)
        {
            _gname = gname;
            _gcover = Convert.ToBase64String(gcover);
            _dev = dev;
            _gprice = gprice;
            _ggenre = ggenre;
        }
        public string Gamename { get { return _gname; } }
        public string Gamecover { get { return _gcover; } }
        public string Dev { get { return _dev; } }
        public int Price { get { return _gprice; } }
        public string Genre { get { return _ggenre; } }
    }
}
