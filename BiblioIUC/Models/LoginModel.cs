using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioIUC.Models
{
    public class LoginModel:BaseModel
    {
        public string Account { get; set; }
        public string Password { get; set; }


        public LoginModel() : base() { }

        private LoginModel(StatusOptions status):base(0, status)
        {
        }

        public LoginModel(string account, string password):this(StatusOptions.Actived)
        {
            Account = account;
            Password = password;
        }
    }
}
