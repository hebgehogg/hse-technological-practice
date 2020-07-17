using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiInfSyst.DBwablon
{
    public class listInventory
    {
        private readonly string _invname;
        private readonly string _gname;
        public listInventory(string invname, string gname)
        {
            _invname = invname;
            _gname = gname;
        }
        public string InventoryName { get { return _invname; } }
        public string GameName { get { return _gname; } }
    }
}
