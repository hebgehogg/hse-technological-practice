namespace WebApiInfSyst.DBwablon
{
    public class GetBuyGame
    {
        private readonly int _gprice;
        private readonly string _gname;
        private readonly string _gcover;
        private readonly string _ggenre;
        public GetBuyGame(string gname, string gcover,string ggenre, int gprice)
        {
            _gname = gname;
            _gcover = gcover;
            _ggenre = ggenre;
            _gprice = gprice;
        }
        public string GameName { get { return _gname; } }
        public string GameCover { get { return _gcover; } }
        public string GameGenre { get { return _ggenre; } }
        public int GamePrice { get { return _gprice; } }
    }
}
