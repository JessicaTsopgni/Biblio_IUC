using System;
using System.Collections.Generic;

namespace BiblioIUC.Entities
{
    public partial class Suggestion
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string File { get; set; }
        public bool IsReaded { get; set; }
        public bool IsSolved { get; set; }
        public DateTime Date { get; set; }

        public virtual User User { get; set; }
    }
}
