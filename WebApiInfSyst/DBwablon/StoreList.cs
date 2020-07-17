using System.Collections.Generic;
using WebApiInfSyst.Models;

namespace WebApiInfSyst.DBwablon
{
    public class StoreList
    {
        private readonly string _giw;
        private readonly List<GamesListStore> _gl;
        private readonly List<InvListStore> _il;
        private readonly List<Genres> _genre;
        private readonly List<WallListStore> _wl;
        public StoreList(string giw, List<GamesListStore> gl, List<Genres> genres)
        {
            _giw = giw;
            _gl = gl;
            _genre = genres;
        }
        public StoreList(string giw, List<InvListStore> il)
        {
            _giw = giw;
            _il = il;
        }
        public StoreList(string giw, List<WallListStore> wl)
        {
            _giw = giw;
            _wl = wl;
        }
        public string GIW { get { return _giw; } }
        public List<GamesListStore> GL { get { return _gl; } }
        public List<InvListStore> IL { get { return _il; } }
        public List<WallListStore> WL { get { return _wl; } }
        public List<Genres> Genre { get { return _genre; } }
    }
}
