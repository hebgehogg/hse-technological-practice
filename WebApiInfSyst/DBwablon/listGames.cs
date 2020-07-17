using System;

namespace WebApiInfSyst.DBwablon
{
    public class listGames
    {
        private readonly string _gname;
        private readonly string _gcover;
        private readonly string _dev;
        private readonly int? _count;
        private readonly int? _prt;
        public listGames(string gname,byte[] gcover, string dev, int? count, int? prt)
        {
            _gname = gname;
            _gcover = Convert.ToBase64String(gcover);
            _dev = dev;
            _count = count;
            _prt = prt;
        }
        public string GameName { get { return _gname; } }
        public string GameCover { get { return _gcover; } }
        public string DeveloperName { get { return _dev; } }
        public int? CountGameBay { get { return _count; } }
        public int? Prt { get { return _prt; } }
    }
}
