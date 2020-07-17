using Microsoft.AspNetCore.Identity;

namespace WebApiInfSyst.DBwablon
{
    public class User
    {
        private readonly string _login;
        private readonly char _type;
        private readonly string _pass;
        private readonly string _fio;
        public User() { }
        public User(string login, string pass)
        {
            _login = login;
            _pass = pass;
        }
        public User(string fio, string login, string pass)
        {
            _login = login;
            _pass = pass;
            _fio = fio;
        }
        public string Login { get { return _login; } }
        public string Pass { get { return _pass; } }
        public string Fio { get { return _fio; } }
    }
}
