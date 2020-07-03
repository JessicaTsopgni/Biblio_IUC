using BiblioIUC.Entities;
using BiblioIUC.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BiblioIUC.Logics.Interfaces
{
    public interface IUserLogic
    {
        Task<UserModel> GetAsync(int id, string mediaFolderPath);
        Task<UserModel> GetAsync(string account, string mediaFolderPath);
        Task<UserModel> GetAsync(HttpContext httpContext, string mediaFolderPath);
        void SignIn(ProfileModel profileModel, HttpContext httpContext);
        void SignOut(HttpContext httpContext);
        Task<ProfileModel> EditProfilAsync(ProfileModel profileModel, HttpRequest request, 
            string mediaFolderPath, string prefixPhotoProfileName);
        Task<ProfileModel> LoginAsync(LoginModel loginModel, HttpContext httpContext, string mediaFolderPath);
        Task<bool> AccountAlreadyExistsAsync(string account, int id);
        Task<UserModel> SetAsync(UserModel userModel, string mediaFolderPath, string prefixPhotoProfileName, bool forLDAP);
        Task<UserModel> AddAsync(UserModel userModel, string mediaFolderPath, string prefixPhotoProfileName);
        Task<IEnumerable<UserModel>> FindAsync(string value, string mediaFolderPath, Expression<Func<User, object>> orderBy, bool withDisabled = false, int pageIndex = 1, int pageSize = 10);
        Task RemoveAsync(int id, string mediaFolderPath);
    }
}