using BiblioIUC.Entities;
using BiblioIUC.Localize;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioIUC.Models
{
    public class DocumentModel:BaseModel
    {

        [Display(Name = "Code", ResourceType = typeof(Text))]
        [Required(ErrorMessageResourceName = "The_fields_x_is_required", ErrorMessageResourceType = typeof(Text))]
        [MaxLength(50, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        [Remote("CodeExists", "Document", AdditionalFields = "Id", ErrorMessageResourceName = "x_already_existant", ErrorMessageResourceType = typeof(Text))]
        public string Code { get; set; }

        [Display(Name = "Title", ResourceType = typeof(Text))]
        [Required(ErrorMessageResourceName = "The_fields_x_is_required", ErrorMessageResourceType = typeof(Text))]
        [MaxLength(100, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        public string Title { get; set; }


        [Display(Name = "Sub_title", ResourceType = typeof(Text))]
        [MaxLength(100, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        public string Subtitle { get; set; }


        [Display(Name = "Author", ResourceType = typeof(Text))]
        [MaxLength(100, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        [Required(ErrorMessageResourceName = "The_fields_x_is_required", ErrorMessageResourceType = typeof(Text))]
        public string Author { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Text))]
        public string Description { get; set; }

        [Display(Name = "Language", ResourceType = typeof(Text))]
        [Required(ErrorMessageResourceName = "The_fields_x_is_required", ErrorMessageResourceType = typeof(Text))]
        [MaxLength(50, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        public string Language { get; set; }

        [Display(Name = "Publish_date", ResourceType = typeof(Text))]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")] 
        public DateTime? PublishDate { get; set; }

        [Display(Name = "Publisher", ResourceType = typeof(Text))]
        [MaxLength(100, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        public string Publisher { get; set; }

        [Display(Name = "Number_of_pages", ResourceType = typeof(Text))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "x_minimum_value_is_y", ErrorMessageResourceType = typeof(Text))]
        public int NumberOfPages { get; set; }

        [Display(Name = "Contributors", ResourceType = typeof(Text))]
        [MaxLength(300, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        public string Contributors { get; set; }

        public bool IsFileUploadedRequired { get => Id == 0; }

        //[Required(ErrorMessageResourceName = "The_fields_x_is_required", ErrorMessageResourceType = typeof(Text))]
        [Display(Name = "The_document", ResourceType = typeof(Text))]
        public IFormFile FileUploaded { get; set; }

        [Display(Name = "Category", ResourceType = typeof(Text))]
        public int CategoryId { get; set; }

        [Display(Name = "Category", ResourceType = typeof(Text))]
        public string CategoryName { get; }

        [Display(Name = "The_document", ResourceType = typeof(Text))]
        public string FileLink { get; protected set; }

        public bool UpdateMetadata { get; set; }

        public DateTime CreateDate { get; set; }

        public IEnumerable<SelectListItem> CategoryModels { get; set; }

        public DocumentModel() : base()
        {

        }

        public DocumentModel(int id, StatusOptions status)
            : base(id, status)
        {
        }


        private DocumentModel(int id, string code, string title, string subtitle, string author,
            string description, string language, DateTime? publishDate, string publisher,
            int numberOfPages, string contributors, int categoryId, DateTime createDate,
            StatusOptions status):this(id, status)
        {
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
            CreateDate = createDate;
        }


        public DocumentModel(Document document, string imageLinkBaseUrl, string fileLinkBaseUrl)
          : this(document?.Id ?? 0, document?.Code, document?.Title,document?.Subtitle, document?.Author,
                document?.Description, document?.Language, document?.PublishDate, document?.Publisher,
                document?.NumberOfPages ?? 0, document?.Contributors, document?.CategoryId ?? 0,
               document?.CreateDate?? DateTime.UtcNow, (StatusOptions)(document?.Status ?? 0))
        {
            CategoryName = document?.Category?.Name;
            ImageLink = !string.IsNullOrEmpty(document?.Image) ? $"{imageLinkBaseUrl}/{document?.Image}" : null;
            FileLink = !string.IsNullOrEmpty(document?.File) ? $"{fileLinkBaseUrl}/{document?.File}" : null;
        }

        public DocumentModel(string code, IEnumerable<CategoryModel> categories, int? categoryId,
            StatusOptions status) : base(0, status)
        {
            Code = code;
            SetCategoryModels(categories, categoryId);
        }

        public void SetCategoryModels(IEnumerable<CategoryModel> categories, int? categoryId = null)
        {
            CategoryModels = categories.Select
            (
                x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                    Selected = categoryId != null && categoryId == x.Id
                }
            );
        }
    }
}
