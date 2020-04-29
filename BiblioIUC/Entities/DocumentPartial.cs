using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioIUC.Entities
{

    public partial class Document
    {
        public Document()
        {

        }

        public Document(int id, string code, string title, string subtitle, string author, string description,
            string language, DateTime? publishDate, string publisher, int numberOfPages,
         string contributors, int categoryId, Category category, string file,
         string image, short status):this()
        {
            Id = id;
            Code = code;
            Title = title;
            Subtitle = subtitle;
            Author = author;
            Description = description;
            Language = language;
            PublishDate = publishDate;
            Publisher = publisher;
            NumberOfPages = numberOfPages;
            Contributors = contributors;
            CategoryId = categoryId;
            Image = image;
            File = file;
            Status = status;
            Category = category;
        }
    }
}
