using System;

namespace WebApiInfSyst.DBwablon
{
    public class WallListStore
    {
        private readonly string _wallPhoto;
        private readonly string _wallName;
        private readonly int _price;
        public WallListStore( byte[] wallPhoto, string wallName, int price)
        {
            _wallPhoto = Convert.ToBase64String(wallPhoto);
            _wallName = wallName;
            _price = price;
        }
        public string WallPhoto { get { return _wallPhoto; } }
        public string WallName { get { return _wallName; } }
        public int Price { get { return _price; } }
    }
}
