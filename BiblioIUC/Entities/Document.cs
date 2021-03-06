﻿using System;
using System.Collections.Generic;

namespace BiblioIUC.Entities
{
    public partial class Document
    {
        public Document()
        {
            UserDocuments = new HashSet<UserDocument>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Authors { get; set; }
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
        public double ReadCount { get; set; }

        public virtual Category Category { get; set; }
        public virtual ICollection<UserDocument> UserDocuments { get; set; }
    }
}
