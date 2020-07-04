using BiblioIUC.Entities;
using BiblioIUC.Localize;
using BiblioIUC.Logics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BiblioIUC.Models
{
    public class SuggestionModel:BaseModel
    {

        [Display(Name = "Subject", ResourceType = typeof(Text))]
        [Required(ErrorMessageResourceName = "The_fields_x_is_required", ErrorMessageResourceType = typeof(Text))]
        [MaxLength(100, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        public string Subject { get; set; }

        [MaxLength(500, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        [Required(ErrorMessageResourceName = "The_fields_x_is_required", ErrorMessageResourceType = typeof(Text))]
        [Display(Name = "Message", ResourceType = typeof(Text))]
        public string Message { get; set; }

        [Display(Name = "Is_readed", ResourceType = typeof(Text))]
        public bool IsRead { get; set; }

        [Display(Name = "Is_solved", ResourceType = typeof(Text))]
        public bool IsSolved { get; set; }


        public  DateTime Date { get; set; }

        [Display(Name = "Attached_file", ResourceType = typeof(Text))]
        public string FileLink { get; set; }
        [Display(Name = "Attached_file", ResourceType = typeof(Text))]
        [JsonIgnore]
        public IFormFile FileUploaded { get; set; }
        public string FileUploadedName { get; set; }
        public UserModel UserModel { get; set; }

        public string IsReadedName => IsRead ? Text.Yes : Text.No;
        public string IsSolvedName => IsSolved ? Text.Yes : Text.No;

        public SuggestionModel() : base()
        {

        }

        private SuggestionModel(int id, StatusOptions status):base(id, status)
        {
        }

        private SuggestionModel(int id, string subject, string message,
            bool isRead, bool isSolved, DateTime date, UserModel userModel) 
            : this(id, StatusOptions.Actived)
        {
            Subject = subject;
            Message = message;
            IsRead = isRead;
            IsSolved = isSolved;
            UserModel = userModel;
            Date = date;
        }

        public SuggestionModel(Suggestion suggestion, string fileLinkBaseUrl, string imageLinkBaseUrl)
            : this(suggestion?.Id ?? 0, suggestion?.Subject, suggestion?.Message,
                  suggestion?.IsReaded ?? false, suggestion?.IsSolved ?? false, suggestion?.Date ?? DateTime.MinValue,
                  new UserModel(suggestion?.User, imageLinkBaseUrl))
        {
            FileLink = !string.IsNullOrEmpty(suggestion?.File) ? $"{fileLinkBaseUrl}/{suggestion?.File}" : null;
        }


    }
}
