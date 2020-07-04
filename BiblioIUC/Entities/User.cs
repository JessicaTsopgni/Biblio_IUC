using System;
using System.Collections.Generic;

namespace BiblioIUC.Entities
{
    public partial class User
    {
        public User()
        {
            Suggestions = new HashSet<Suggestion>();
            UserDocuments = new HashSet<UserDocument>();
        }

        public int Id { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public short Role { get; set; }
        public string Image { get; set; }
        public short Status { get; set; }

        public virtual ICollection<Suggestion> Suggestions { get; set; }
        public virtual ICollection<UserDocument> UserDocuments { get; set; }
    }
}
