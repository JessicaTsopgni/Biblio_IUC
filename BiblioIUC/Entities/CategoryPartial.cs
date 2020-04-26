using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioIUC.Entities
{

    public partial class Category
    {

        public Category(int id, string name, string description, int? categoryParentId,
           Category categoryParent, string image, short status):this()
        {
            Id = id;
            Name = name;
            Description = description;
            CategoryParentId = categoryParentId;
            CategoryParent = categoryParent;
            Image = image;
            Status = status;
        }
    }
}
