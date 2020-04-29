using BiblioIUC.Entities;
using BiblioIUC.Localize;
using BiblioIUC.Logics.Interfaces;
using BiblioIUC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BiblioIUC.Logics
{
    public class UserLogic:IUserLogic
    {
        private readonly BiblioEntities biblioEntities;
        private readonly IWebHostEnvironment env;

        public UserLogic(BiblioEntities biblioEntities, IWebHostEnvironment env)
        {
            this.biblioEntities = biblioEntities;
            this.env = env;
        }

        public async Task<UserModel> GetAsync(int id, string mediaFolderPath)
        {
            var user = await biblioEntities.Users.FindAsync(id);

            return GetUserModel(user, mediaFolderPath);
        }

        public async Task<UserModel> GetAsync(string account, string mediaFolderPath)
        {
            var user = await biblioEntities.Users.SingleOrDefaultAsync
            (
                x => x.Account.Equals(account, StringComparison.OrdinalIgnoreCase)
            );
            return GetUserModel(user, mediaFolderPath);
        }

        public async Task<UserModel> GetAsync(HttpContext httpContext, string mediaFolderPath)
        {
            return await GetAsync
            (
                httpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value,
                mediaFolderPath
            );
        }

        private string GetPassword(string oldPassword, string newPassword)
        {
            //TO DO: encrypt password
            return string.IsNullOrEmpty(newPassword)? oldPassword : newPassword;
        }

        public async Task<ProfileModel> LoginAsync(LoginModel loginModel, 
            HttpContext httpContext, string mediaFolderPath)
        {
            if (loginModel == null)
                throw new ArgumentNullException(nameof(loginModel));
            var user = await biblioEntities.Users.SingleOrDefaultAsync
            (
                x =>
                x.Account.Equals(loginModel.Account, StringComparison.OrdinalIgnoreCase) &&
                x.Password.Equals(loginModel.Password, StringComparison.Ordinal)
            );
            if(user == null)
                throw new MemberAccessException(Text.Account_or_password_is_incorrect);

            if (user.Status == (short)StatusOptions.Actived)
            {
                var profileModel = new ProfileModel(user, mediaFolderPath);
                SignIn(profileModel, httpContext);
                return profileModel;
            }
            else if (user.Status == (short)StatusOptions.Disabled)
                throw new MemberAccessException(Text.Your_account_has_been_disabled);
            else if (user.Status == (short)StatusOptions.InProcess)
                throw new MemberAccessException(Text.Your_account_has_been_not_yet_actived);
            else
                throw new MemberAccessException(Text.Account_or_password_is_incorrect);
        }

        public async void SignIn(ProfileModel profileModel, HttpContext httpContext)
        {
            var claims = new[] 
            {
                   new Claim(ClaimTypes.NameIdentifier, profileModel.Account),
                   new Claim(ClaimTypes.Name, profileModel.FullName),
                   new Claim(ClaimTypes.Role, profileModel.RoleValue.ToString()),
                   new Claim("User.ImageLink", profileModel.ImageLink ?? "")
            };

            var identity = new ClaimsIdentity
            (
                claims, 
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            await httpContext.SignInAsync
            (
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            //httpContext.Session.SetString("User", JsonConvert.SerializeObject(userModel));
        }

        public async void SignOut(HttpContext httpContext)
        {
            await httpContext.SignOutAsync();
        }

        public async Task<ProfileModel> EditProfilAsync(ProfileModel profileModel,
            HttpRequest request, string mediaFolderPath, string prefixPhotoProfileName)
        {
            string newFileName = null;
            var mediaBasePath = Path.Combine(env.WebRootPath, mediaFolderPath?.Replace("~/", string.Empty));
            try
            {
                var currentUser = await biblioEntities.Users.SingleOrDefaultAsync
                (
                    x => x.Account.Equals(request.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value, StringComparison.OrdinalIgnoreCase)
                );
                if (currentUser == null)
                    throw new KeyNotFoundException("Profile");


                if (Tools.OssFile.HasImage(profileModel.ImageUploaded))
                {
                    newFileName = Tools.OssFile.SaveImage(profileModel.ImageUploaded, 300, 300, 100 * 1024, 200 * 1024, prefixPhotoProfileName, mediaBasePath); ;
                    Tools.OssFile.DeleteFile(currentUser.Image, mediaBasePath);
                }
                else if (!string.IsNullOrEmpty(currentUser.Image) && profileModel.DeleteImage)
                {
                    Tools.OssFile.DeleteFile(currentUser.Image, mediaBasePath);
                }
                else
                {
                    newFileName = currentUser.Image;
                }
                profileModel.Password = GetPassword(currentUser.Password, profileModel.Password);
                User newUser = new User
                (
                    currentUser.Id,
                    currentUser.Account,
                    profileModel.Password,
                    currentUser.FullName,
                    currentUser.Role,
                    newFileName,
                    currentUser.Status
                );

                biblioEntities.Entry(currentUser).CurrentValues.SetValues(newUser);
                await biblioEntities.SaveChangesAsync();
                profileModel = new ProfileModel(newUser, mediaFolderPath);
                return profileModel;
            }
            catch (Exception ex)
            {
                if (Tools.OssFile.HasImage(profileModel.ImageUploaded) && !string.IsNullOrEmpty(newFileName))
                    Tools.OssFile.DeleteFile(newFileName, mediaBasePath);
                throw ex;
            }
        }
        public async Task<bool> AccountAlreadyExistsAsync(string account, int id)
        {
            try
            {
                return await biblioEntities.Users.SingleOrDefaultAsync
                (
                    x =>
                    x.Account.Equals(account, StringComparison.OrdinalIgnoreCase) &&
                    x.Id != id
                ) != null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private UserModel GetUserModel(User user, string mediaFolderPath)
        {
            return user != null ? new UserModel
            (
                user,
                mediaFolderPath
            ) : null;
        }
    }
}
