using BiblioIUC.Localize;
using BiblioIUC.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioIUC.Entities
{
    public partial class User
    {       

        public User() {}

        public User(int id, string account, string password, string fullName, 
            short role, string image, short status):this()
        {
            Id = id;
            Account = account;
            Password = password;
            FullName = fullName;
            Role = role;
            Image = image;
            Status = status;
        }
    }
}
