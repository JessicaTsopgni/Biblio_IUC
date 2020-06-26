using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioIUC.Models
{
    public class UserLDAPModel
    {
        public string FullName { get; set; }
        public string DisplayName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public RoleOptions Role { get; set; }

        public UserLDAPModel()
        {

        }

        public UserLDAPModel(string fullName, string displayName, string username, 
            string password, RoleOptions role)
        {
            FullName = fullName;
            DisplayName = displayName;
            Username = username;
            Password = password;
            Role = role;
        }
    }
}
