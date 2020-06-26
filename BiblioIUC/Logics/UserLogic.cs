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
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BiblioIUC.Logics
{
    public class UserLogic:IUserLogic
    {
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_PAGE_SIZE = 10;

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
            if (string.IsNullOrEmpty(newPassword))
                return oldPassword;

            var password = Tools.MD5.Hash(newPassword);
            return (oldPassword?.Equals(newPassword) ?? false) ? oldPassword : password;
        }

        public async Task<ProfileModel> LoginAsync(LoginModel loginModel, 
            HttpContext httpContext, string mediaFolderPath)
        {
            if (loginModel == null)
                throw new ArgumentNullException(nameof(loginModel));
            var password = GetPassword(null, loginModel.Password);
            var user = await biblioEntities.Users.SingleOrDefaultAsync
            (
                x =>
                x.Account.Equals(loginModel.Account, StringComparison.OrdinalIgnoreCase) &&
                x.Password.Equals(password, StringComparison.Ordinal)
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
                   new Claim("User.ImageLink", profileModel.ImageLink ?? ""),
                   new Claim("User.Id", profileModel.Id.ToString())
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
            string newImageName = null;
            var mediaAbsoluteBasePath = Path.Combine(env.WebRootPath, mediaFolderPath?.Replace("~/", string.Empty));
            try
            {
                string oldImageName = null;
                var currentUser = await biblioEntities.Users.SingleOrDefaultAsync
                (
                    x => x.Account.Equals(request.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value, StringComparison.OrdinalIgnoreCase)
                );
                if (currentUser == null)
                    throw new KeyNotFoundException("Profile");

                bool deleteCurrentImage = false;
                oldImageName = currentUser.Image;
                if (Tools.OssFile.HasImage(profileModel.ImageUploaded))
                {
                    newImageName = Tools.OssFile.SaveImage(profileModel.ImageUploaded, 300, 300, 100 * 1024, 200 * 1024, prefixPhotoProfileName, mediaAbsoluteBasePath); ;
                    deleteCurrentImage = true;
                }
                else if (!string.IsNullOrEmpty(currentUser.Image) && profileModel.DeleteImage)
                {
                    deleteCurrentImage = true;
                }
                else
                {
                    newImageName = currentUser.Image;
                }

                var newPassword = GetPassword(currentUser.Password, profileModel.Password);
                User newUser = new User
                (
                    currentUser.Id,
                    currentUser.Account,
                    newPassword,
                    currentUser.FullName,
                    currentUser.Role,
                    newImageName,
                    currentUser.Status
                );

                biblioEntities.Entry(currentUser).CurrentValues.SetValues(newUser);
                await biblioEntities.SaveChangesAsync();

                if (deleteCurrentImage)
                    Tools.OssFile.DeleteFile(oldImageName, mediaAbsoluteBasePath);

                profileModel = new ProfileModel(newUser, mediaFolderPath);
                return profileModel;
            }
            catch (Exception ex)
            {
                if (Tools.OssFile.HasImage(profileModel.ImageUploaded) && !string.IsNullOrEmpty(newImageName))
                    Tools.OssFile.DeleteFile(newImageName, mediaAbsoluteBasePath);
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


        public async Task<UserModel> AddAsync(UserModel userModel,
            string mediaFolderPath, string prefixPhotoProfileName)
        {
            var mediaAbsoluteBasePath = Path.Combine(env.WebRootPath, mediaFolderPath.Replace("~/", string.Empty));
            string newImageName = null;
            try
            {
                if (userModel == null)
                    throw new ArgumentNullException("userModel");


                if (Tools.OssFile.HasImage(userModel.ImageUploaded))
                {
                    newImageName = Tools.OssFile.SaveImage(userModel.ImageUploaded, 300, 300, 100 * 1024, 200 * 1024, prefixPhotoProfileName, mediaAbsoluteBasePath); ;
                }

                var newPassword = GetPassword(null, userModel.Password);
                User newUser = new User
                (
                    userModel.Id,
                    userModel.Account,
                    newPassword,
                    userModel.FullName,
                    (short)userModel.Role,
                    newImageName,
                    (short)userModel.Status
                );

                biblioEntities.Users.Add(newUser);
                await biblioEntities.SaveChangesAsync();
                return new UserModel(newUser, mediaFolderPath);
            }
            catch (Exception ex)
            {
                if (Tools.OssFile.HasImage(userModel.ImageUploaded) && !string.IsNullOrEmpty(newImageName))
                    Tools.OssFile.DeleteFile(newImageName, mediaAbsoluteBasePath);
                throw ex;
            }
        }


        public async Task<UserModel> SetAsync(UserModel userModel,
            string mediaFolderPath, string prefixPhotoProfileName, bool forLDAP)
        {
            string newImageName = null;
            var mediaAbsoluteBasePath = Path.Combine(env.WebRootPath, mediaFolderPath?.Replace("~/", string.Empty));
            try
            {
                string oldImageName = null;
                if (userModel == null)
                    throw new ArgumentNullException("userModel");
                bool deleteCurrentImage = false;
                var currentUser = await biblioEntities.Users.FindAsync(userModel.Id);
                if (currentUser == null)
                    throw new KeyNotFoundException("User");


                oldImageName = currentUser.Image;
                if (Tools.OssFile.HasImage(userModel.ImageUploaded))
                {
                    newImageName = Tools.OssFile.SaveImage(userModel.ImageUploaded, 300, 300, 100 * 1024, 200 * 1024, prefixPhotoProfileName, mediaAbsoluteBasePath); ;
                    deleteCurrentImage = true;
                }
                else if (!string.IsNullOrEmpty(currentUser.Image) && userModel.DeleteImage)
                {
                    deleteCurrentImage = true;
                }
                else
                {
                    newImageName = currentUser.Image;
                }

                var newPassword = GetPassword(forLDAP ? null : currentUser.Password, userModel.Password);
                User newUser = new User
                (
                    userModel.Id,
                    userModel.Account,
                    newPassword,
                    userModel.FullName,
                    (short)userModel.Role,
                    newImageName,
                    (short)userModel.Status
                );

                biblioEntities.Entry(currentUser).CurrentValues.SetValues(newUser);
                await biblioEntities.SaveChangesAsync();

                if (deleteCurrentImage)
                    Tools.OssFile.DeleteFile(oldImageName, mediaAbsoluteBasePath);

                return new UserModel(newUser, mediaFolderPath);
            }
            catch (Exception ex)
            {
                if (Tools.OssFile.HasImage(userModel.ImageUploaded) && !string.IsNullOrEmpty(newImageName))
                    Tools.OssFile.DeleteFile(newImageName, mediaAbsoluteBasePath);
                throw ex;
            }
        }

        public async Task<IEnumerable<UserModel>> FindAsync(string value, string mediaFolderPath,
            Expression<Func<User, object>> orderBy,
            bool withDisabled = false, int pageIndex = DEFAULT_PAGE_INDEX,
            int pageSize = DEFAULT_PAGE_SIZE)
        {
            value = value?.ToLower() ?? string.Empty;
            pageIndex = pageIndex < DEFAULT_PAGE_INDEX ? DEFAULT_PAGE_INDEX : pageIndex;
            pageSize = pageSize < pageIndex ? DEFAULT_PAGE_SIZE : pageSize;
            var query = biblioEntities.Users
            .Where(x => true);
            if (!string.IsNullOrWhiteSpace(value))            
            {
                query = query.Where
                (
                    x =>
                    (x.Account.ToLower().Contains(value) || value.Contains(x.Account)) ||
                    (x.FullName != null && (x.FullName.ToLower().Contains(value) || value.Contains(x.FullName)))
                );
            }

            if (!withDisabled)
                query = query.Where(x => x.Status == (short)StatusOptions.Actived);

            return
            (
                await query.OrderBy(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArrayAsync()
            )
            .Select
            (
                x => GetUserModel(x, mediaFolderPath)
            ).ToArray();
        }

    }
}
