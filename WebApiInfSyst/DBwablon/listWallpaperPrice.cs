using System;

namespace WebApiInfSyst.DBwablon
{
    public class listWallpaperPrice
    {
        private readonly string _wname;
        private readonly DateTime _sdate;
        private readonly DateTime _edate;
        private readonly int _price;
        public listWallpaperPrice(string wname, DateTime sdate, DateTime edate, int price)
        {
            _wname = wname;
            _sdate = sdate;
            _edate = edate;
            _price = price;
        }
        public string WallpapersName { get { return _wname; } }
        public DateTime WallpaperStartDate { get { return _sdate; } }
        public DateTime WallpaperEndDate { get { return _edate; } }
        public int Price { get { return _price; } }
    }
}
