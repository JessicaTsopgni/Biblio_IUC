using BiblioIUC.Models;
using Microsoft.AspNetCore.Http;
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
    }
}