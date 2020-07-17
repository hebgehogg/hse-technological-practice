using System.Collections.Generic;

namespace WebApiInfSyst.DBwablon
{
    public class GetLib
    {
        private readonly List<LibConst> _list;
        public GetLib(List<LibConst> list)
        {
            _list = list;
        }
        public List<LibConst> List { get { return _list; } }
    }
}
