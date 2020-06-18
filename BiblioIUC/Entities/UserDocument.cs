using System;
using System.Collections.Generic;

namespace BiblioIUC.Entities
{
    public partial class UserDocument
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public int DocumentId { get; set; }
        public int LastPageNumber { get; set; }
        public DateTime ReadDate { get; set; }

        public virtual Document Document { get; set; }
        public virtual User User { get; set; }
    }
}
