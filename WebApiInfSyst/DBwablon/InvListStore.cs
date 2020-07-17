using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiInfSyst.DBwablon
{
    public class InvListStore
    {
        private readonly string _iname;
        private readonly string _gname;
        private readonly int _iprice;
        public InvListStore(string iname, string gname, int iprice)
        {
            _iname = iname;
            _gname = gname;
            _iprice = iprice;
        }
        public string InventoryName { get { return _iname; } }
        public string GameName { get { return _gname; } }
        public int Price { get { return _iprice; } }
    }
}
