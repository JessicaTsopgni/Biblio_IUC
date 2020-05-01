using System;
using System.Collections.Generic;

namespace BiblioIUC.Entities
{
    public partial class Document
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public DateTime? PublishDate { get; set; }
        public string Publisher { get; set; }
        public int NumberOfPages { get; set; }
        public string Contributors { get; set; }
        public int CategoryId { get; set; }
        public string Image { get; set; }
        public string File { get; set; }
        public DateTime CreateDate { get; set; }
        public short Status { get; set; }

        public virtual Category Category { get; set; }
    }
}
