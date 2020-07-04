using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioIUC.Entities
{

    public partial class Suggestion
    {
        public Suggestion()
        {

        }

        public Suggestion(int id, string subject, string message, 
            string file, bool isReaded, bool isSolved, DateTime date, int userId, User user):this()
        {
            Id = id;
            Subject = subject;
            Message = message;
            File = file;
            IsReaded = isReaded;
            IsSolved = isSolved;
            Date = date;
            UserId = userId;
            User = user;
        }
    }
}
