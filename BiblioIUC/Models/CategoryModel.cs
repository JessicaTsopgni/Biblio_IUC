﻿using BiblioIUC.Entities;
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
    public class CategoryModel:BaseModel
    {

        [Display(Name = "Name", ResourceType = typeof(Text))]
        [Required(ErrorMessageResourceName = "The_fields_x_is_required", ErrorMessageResourceType = typeof(Text))]
        [MaxLength(50, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        [Remote("NameExists", "Category", AdditionalFields = "Id", ErrorMessageResourceName = "x_already_existant", ErrorMessageResourceType = typeof(Text))]
        public string Name { get; set; }

        [MaxLength(200, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        [Display(Name = "Description", ResourceType = typeof(Text))]
        public string Description { get; set; }

        [Display(Name = "Parent_category", ResourceType = typeof(Text))]
        public int? CategoryParentId { get; set; }

        [Display(Name = "Parent_category", ResourceType = typeof(Text))]
        public string CategoryParentName { get; }

        public IEnumerable<SelectListItem> CategoryParentModels { get; set; }

        public int NumberOfDocuments { get; }
        public string NumberOfDocumentsFormated { get => NumberOfDocuments.ToString("N0"); }

        public int NumberOfSubCategories { get; }
        public string NumberOfSubCategoriesFormated { get => NumberOfSubCategories.ToString("N0"); }

        public CategoryModel() : base()
        {

        }

        private CategoryModel(int id, StatusOptions status):base(id, status)
        {

        }

        private CategoryModel(int id, string name, string description,
            int? categoryParentId, StatusOptions status) 
            : this(id, status)
        {
            Name = name;
            Description = description;
            CategoryParentId = categoryParentId;
        }

        public CategoryModel(Category category, string imageLinkBaseUrl)
            : this(category?.Id ?? 0, category?.Name, category?.Description, category?.CategoryParentId,
                  (StatusOptions)(category?.Status ?? 0))
        {
            CategoryParentName = category?.CategoryParent?.Name;
            ImageLink = !string.IsNullOrEmpty(category?.Image) ? $"{imageLinkBaseUrl}/{category?.Image}" : null;
            NumberOfDocuments = new Random().Next(0, 10000);
            NumberOfSubCategories = category?.InverseCategoryParent.Count() ?? 0;
        }

        public CategoryModel(IEnumerable<CategoryModel> categoryParents, int? categoryParentId,
            StatusOptions status):base(0, status)
        {
            SetCategoryParentModels(categoryParents, categoryParentId);
        }

        public void SetCategoryParentModels(IEnumerable<CategoryModel> categoryParents, int? categoryParentId = null)
        {
            CategoryParentModels = categoryParents.Select
            (
                x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                    Selected = categoryParentId != null && categoryParentId == x.Id
                }
            );
        }
    }
}
