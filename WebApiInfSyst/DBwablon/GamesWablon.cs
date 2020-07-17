using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;

namespace WebApiInfSyst.DBwablon
{
    public class GamesWablon
    {
        private readonly int _GameID;
        private readonly string _GameName;
        private readonly string _GameCover;
        private readonly string _DeveloperName;
        private readonly int? _CountGameBay;
        private readonly int? _Prt;
        public GamesWablon(int GameID, string GameName, byte[] GameCover, int? CountGameBay, string DeveloperName, int? Prt)
        {
            _GameID = GameID;
            _GameName = GameName;
            _GameCover = Convert.ToBase64String(GameCover);
            _CountGameBay = CountGameBay;
            _DeveloperName = DeveloperName;
            _Prt = Prt;
        }
        public int GameID { get { return _GameID; } }
        public string GameName { get { return _GameName; } }
        public string GameCover { get { return _GameCover; } }
        public string DeveloperName { get { return _DeveloperName; } }
        public int? CountGameBay { get { return _CountGameBay; } }
        public int? Prt { get { return _Prt; } }
    }
}
