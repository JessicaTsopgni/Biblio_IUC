using BiblioIUC.Entities;
using BiblioIUC.Localize;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioIUC.Models
{
    public class ProfileModel:BaseModel
    {
        public string Account { get; }

        [MinLength(8, ErrorMessageResourceName = "The_field_x_must_have_at_least_y_characters", ErrorMessageResourceType = typeof(Text))]
        [MaxLength(50, ErrorMessageResourceName = "The_field_x_must_have_most_y_characters", ErrorMessageResourceType = typeof(Text))]
        [Display(Name = "Your_password", ResourceType = typeof(Text))]
        public string Password { get; set; }

        [Display(Name = "Repeat_your_password", ResourceType = typeof(Text))]
        [Compare("Password", ErrorMessageResourceName = "x_and_y_doesn_t_matches", ErrorMessageResourceType = typeof(Text))]
        public string ConfirmPassword { get; set; }

        public string FullName { get; }

        public RoleOptions RoleValue { get; }

        public ProfileModel():base() {}

        private ProfileModel(int id, string account, string password,  string fullname, RoleOptions roleValue,
            string imageLink) :base(id, StatusOptions.Actived)
        {
            Account = account;
            Password = password;
            ConfirmPassword = password;
            FullName = fullname;
            RoleValue = roleValue;
            ImageLink = imageLink;
        }

        public ProfileModel(User user, string imageLinkBaseUrl)
            :this(user?.Id ?? 0, user?.Account, user?.Password, user?.FullName, (RoleOptions)(user?.Role ?? 0),
                 !string.IsNullOrEmpty(user?.Image) ? $"{imageLinkBaseUrl}/{user.Image}" : null)
        {
        }

        public ProfileModel(UserModel userModel)
           : this(userModel?.Id ?? 0, userModel?.Account, userModel?.Password, userModel?.FullName, 
                 userModel?.Role ?? RoleOptions.Librarian, userModel?.ImageLink)
        {
        }
    }
}
