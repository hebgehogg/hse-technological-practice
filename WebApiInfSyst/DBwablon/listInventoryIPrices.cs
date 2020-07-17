using System;

namespace WebApiInfSyst.DBwablon
{
    public class listInventoryIPrices
    {
        private readonly string _invname;
        private readonly DateTime _sdate;
        private readonly DateTime _edate;
        private readonly int _price;
        public listInventoryIPrices(string invname, DateTime sdate, DateTime edate, int price)
        {
            _invname = invname;
            _sdate = sdate;
            _edate = edate;
            _price = price;
        }
        public string InventoryName { get { return _invname; } }
        public DateTime GameStartDate { get { return _sdate; } }
        public DateTime GameEndDate { get { return _edate; } }
        public int Price { get { return _price; } }
    }
}
