using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioIUC.Models
{
    public enum MessageOptions
    {
        None,
        Danger,
        Warning,
        Success,
        Info
    }

    public enum StatusOptions
    {
        Deleted = -2,
        InProcess = -1,
        Disabled = 0,
        Actived = 1,
        Ended = 2
    }

    public enum RoleOptions
    {
        Admin = 0,
        Student = 1,
        Teacher = 2
    }
}
