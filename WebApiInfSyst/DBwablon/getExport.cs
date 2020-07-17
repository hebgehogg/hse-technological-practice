using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiInfSyst.Views.Home
{
    public class getExport
    {
        private readonly string _fio;
        private readonly int _pfio;
        public getExport(string fio, int pfio)
        {
            _fio = fio;
            _pfio = pfio;
        }
        public string ClientName { get { return _fio; } }
        public int ClientPrch { get { return _pfio; } }
    }
}
