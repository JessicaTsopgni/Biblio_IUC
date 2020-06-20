using BiblioIUC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioIUC.Logics.Interfaces
{
    public interface ILDAPAuthenticationService
    {
        UserLDAPModel Login(string username, string password);
    }
}
