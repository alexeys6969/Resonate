using Resonate.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resonate.Services
{
    public class TokenStorage
    {
        private static TokenStorage _instance;

        public static TokenStorage Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TokenStorage();
                return _instance;
            }
        }

        private string _token;
        private DateTime _expiration;
        private Employees _currentUser;

        public string Token => _token;
        public DateTime Expiration => _expiration;
        public Employees CurrentUser => _currentUser;

        public bool IsAuthenticated
        {
            get
            {
                return !string.IsNullOrEmpty(_token) && _expiration > DateTime.UtcNow;
            }
        }

        private TokenStorage() { }

        public void SaveToken(string token, DateTime expiration, Employees user)
        {
            _token = token;
            _expiration = expiration;
            _currentUser = user;
        }

        public void ClearToken()
        {
            _token = null;
            _expiration = DateTime.MinValue;
            _currentUser = null;
        }
    }
}
