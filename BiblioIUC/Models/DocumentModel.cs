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

        [Display(Name = "ISBN", ResourceType = typeof(Text))]
        [Required(ErrorMessageResourceName = "The_fields_x_is_required", ErrorMessageResourceType = typeof(Text))]
        [MaxLength(50, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        [Remote("IsbnExists", "Document", AdditionalFields = "Id", ErrorMessageResourceName = "x_already_existant", ErrorMessageResourceType = typeof(Text))]
        public string Isbn { get; set; }

        [Display(Name = "Title", ResourceType = typeof(Text))]
        [Required(ErrorMessageResourceName = "The_fields_x_is_required", ErrorMessageResourceType = typeof(Text))]
        [MaxLength(100, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        public string Title { get; set; }


        [Display(Name = "Sub_title", ResourceType = typeof(Text))]
        [MaxLength(100, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        public string Subtitle { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Text))]
        [Required(ErrorMessageResourceName = "The_fields_x_is_required", ErrorMessageResourceType = typeof(Text))]
        [MaxLength(500, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        public string Description { get; set; }

        [Display(Name = "Language", ResourceType = typeof(Text))]
        [Required(ErrorMessageResourceName = "The_fields_x_is_required", ErrorMessageResourceType = typeof(Text))]
        [MaxLength(50, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        public string Language { get; set; }

        [Display(Name = "Publish_date", ResourceType = typeof(Text))]
        public DateTime PublishDate { get; set; }

        [Display(Name = "Publisher", ResourceType = typeof(Text))]
        [Required(ErrorMessageResourceName = "The_fields_x_is_required", ErrorMessageResourceType = typeof(Text))]
        [MaxLength(100, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        public string Publisher { get; set; }

        [Display(Name = "Number_of_pages", ResourceType = typeof(Text))]
        public int NumberOfPages { get; set; }

        [Display(Name = "Contributors", ResourceType = typeof(Text))]
        [MaxLength(300, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        public string Contributors { get; set; }

        [Display(Name = "The_document", ResourceType = typeof(Text))]
        [Required(ErrorMessageResourceName = "The_fields_x_is_required", ErrorMessageResourceType = typeof(Text))]
        public IFormFile FileUploaded { get; set; }


        [Display(Name = "Category", ResourceType = typeof(Text))]
        public int CategoryId { get; set; }

        [Display(Name = "Category", ResourceType = typeof(Text))]
        public string CategoryName { get; }

        public IEnumerable<SelectListItem> CategoryModels { get; set; }

        public DocumentModel() : base()
        {

        }

        public DocumentModel(int id, StatusOptions status)
            : base(id, status)
        {
        }

        public DocumentModel(Document document, string imageLinkBaseUrl)
          : this(document?.Id ?? 0, 
                (StatusOptions)(document?.Status ?? 0))
        {
            ImageLink = !string.IsNullOrEmpty(document?.Image) ? $"{imageLinkBaseUrl}/{document?.Image}" : null;
        }

        public DocumentModel(IEnumerable<CategoryModel> categories, int? categoryId,
            StatusOptions status) : base(0, status)
        {
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
