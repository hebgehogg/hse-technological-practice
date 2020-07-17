using System;

namespace WebApiInfSyst.DBwablon
{
    public class listWallpapers
    {
        private readonly string _wname;
        private readonly string _wphoto;
        public listWallpapers(string wname, byte[] wphoto)
        {
            _wname = wname;
            _wphoto = Convert.ToBase64String(wphoto);
        }
        public string WallpapersName { get { return _wname; } }
        public string WallpapersPhoto { get { return _wphoto; } }
    }
}
