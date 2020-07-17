using System;

namespace WebApiInfSyst.DBwablon
{
    public class listGemesPrices
    {
        private readonly string _gname;
        private readonly DateTime _sdate;
        private readonly DateTime _edate;
        private readonly int _price;
        public listGemesPrices(string gname, DateTime sdate, DateTime edate, int price)
        {
            _gname = gname;
            _sdate = sdate;
            _edate = edate;
            _price = price;
        }
        public string Game { get { return _gname; } }
        public DateTime GameStartDate { get { return _sdate; } }
        public DateTime GameEndDate { get { return _edate; } }
        public int Price { get { return _price; } }
    }
}
