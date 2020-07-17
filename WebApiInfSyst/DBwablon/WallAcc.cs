using System;

namespace WebApiInfSyst.DBwablon
{
    public class WallAcc
    {
        private readonly int _walid;
        private readonly string _walp;
        public WallAcc(int walid, byte[] walp)
        {
            _walid = walid;
            _walp = Convert.ToBase64String(walp);
        }
        public int WallpaperID { get { return _walid; } }
        public string WallpaperPhoto { get { return _walp; } }
    }
}
