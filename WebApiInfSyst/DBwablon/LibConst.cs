using System.Collections.Generic;

namespace WebApiInfSyst.DBwablon
{
    public class LibConst
    {
        private readonly string _gname;
        private readonly string _gdev;
        private readonly List<string> _ginv;
        public LibConst(string gname, string gdev, List<string> ginv)
        {
            _gname = gname;
            _gdev = gdev;
            _ginv = ginv;
        }
        public string GameName { get { return _gname; } }
        public string DeveloperName { get { return _gdev; } }
        public List<string> GameInv { get { return _ginv; } }
    }
}
