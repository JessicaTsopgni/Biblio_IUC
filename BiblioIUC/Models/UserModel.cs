using BiblioIUC.Entities;
using BiblioIUC.Localize;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioIUC.Models
{
    public class UserModel:BaseModel
    {

        [Display(Name = "Account", ResourceType = typeof(Text))]
        [Required(ErrorMessageResourceName = "The_fields_x_is_required", ErrorMessageResourceType = typeof(Text))]
        [MaxLength(50, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        [Remote("AccountExists", "Account", AdditionalFields = "Id", ErrorMessageResourceName = "x_already_existant", ErrorMessageResourceType = typeof(Text))]
        public string Account { get; set; }

        [Required(ErrorMessageResourceName = "The_fields_x_is_required", ErrorMessageResourceType = typeof(Text))]
        [MinLength(8, ErrorMessageResourceName = "The_field_x_must_have_at_least_y_characters", ErrorMessageResourceType = typeof(Text))]
        [MaxLength(50, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        [Display(Name = "Password", ResourceType = typeof(Text))]
        public string Password { get; set; }

        [Display(Name = "Repeat_password", ResourceType = typeof(Text))]
        [Compare("Password", ErrorMessageResourceName = "x_and_y_doesn_t_matches", ErrorMessageResourceType = typeof(Text))]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Your_full_name", ResourceType = typeof(Text))]
        [MaxLength(100, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        [Required(ErrorMessageResourceName = "The_fields_x_is_required", ErrorMessageResourceType = typeof(Text))]
        public string FullName { get; set; }

        public RoleOptions Role { get; set; }

        public UserModel():base()
        {

        }
        private UserModel(int id, StatusOptions status) : base(id, status)
        {

        }
        private UserModel(int id, string account, string password, string fullName, 
            RoleOptions role, StatusOptions status) :this(0, status)
        {
            Id = id;
            Account = account;
            Password = password;
            FullName = fullName;
            Role = role;
        }

        public UserModel(User user, string imageLinkBaseUrl)
            : this(user?.Id ?? 0, user?.Account, user?.Password, user?.FullName,
                  (RoleOptions)(user?.Role ?? 0), (StatusOptions)(user?.Status ?? 0))
        {
            ImageLink = !string.IsNullOrEmpty(user?.Image) ? $"{imageLinkBaseUrl}/{user.Image}" : null;
        }


        public UserModel(UserLDAPModel userLDAPModel)
            : this(0, userLDAPModel?.Username, userLDAPModel?.Password, userLDAPModel?.FullName,
                 userLDAPModel != null ? userLDAPModel.Role : RoleOptions.Student, StatusOptions.Actived)
        {
        }
    }
}
