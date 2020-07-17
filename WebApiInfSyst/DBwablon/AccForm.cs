using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiInfSyst.DBwablon
{
    public class AccForm
    {
        private readonly int _gcount;
        private readonly int _icount;
        private readonly string _mainw;
        private readonly List<WallAcc> _wall;
        public AccForm(int gcount, int icount, string mainw, List<WallAcc> wall)
        {
            _gcount = gcount;
            _icount = icount;
            _mainw = mainw;
            _wall = wall;
        }
        public int GameCount { get { return _gcount; } }
        public int InvCount { get { return _icount; } }
        public string MainWall { get { return _mainw; } }
        public List<WallAcc> Wall { get { return _wall; } }
    }
}
