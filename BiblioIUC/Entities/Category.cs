using System;
using System.Collections.Generic;

namespace BiblioIUC.Entities
{
    public partial class Category
    {
        public Category()
        {
            Documents = new HashSet<Document>();
            InverseCategoryParent = new HashSet<Category>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? CategoryParentId { get; set; }
        public string Image { get; set; }
        public short Status { get; set; }

        public virtual Category CategoryParent { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<Category> InverseCategoryParent { get; set; }
    }
}
