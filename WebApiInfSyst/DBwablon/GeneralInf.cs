using System.Collections.Generic;

namespace WebApiInfSyst.DBwablon
{
    public class GeneralInf
    {
        //private readonly User _us;
        private readonly List<GamesWablon> _gw;
        public GeneralInf(List<GamesWablon> gw)//User us,
        {
            //_us = us;
            _gw = gw;
        }
        //public User User { get { return _us; } }
        public List<GamesWablon> GW { get { return _gw; } }
    }
}
