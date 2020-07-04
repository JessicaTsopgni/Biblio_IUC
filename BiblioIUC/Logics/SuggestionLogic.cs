using BiblioIUC.Entities;
using BiblioIUC.Localize;
using BiblioIUC.Logics.Interfaces;
using BiblioIUC.Logics.Tools;
using BiblioIUC.Models;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace BiblioIUC.Logics
{

    public class SuggestionLogic : ISuggestionLogic
    {
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_PAGE_SIZE = 10;

        private readonly BiblioEntities biblioEntities;
        private readonly IWebHostEnvironment env;

        public SuggestionLogic(BiblioEntities biblioEntities, IWebHostEnvironment env)
        {
            this.biblioEntities = biblioEntities;
            this.env = env;
        }

        public async Task<IEnumerable<SuggestionModel>> FindAsync(string value, int? userId, string mediaFolderPath,
            Expression<Func<Suggestion, object>> orderBy,
            Expression<Func<Suggestion, object>> orderByDescending, int pageIndex = DEFAULT_PAGE_INDEX, 
            int pageSize = DEFAULT_PAGE_SIZE)
        {
            value = value?.ToLower() ?? string.Empty;
            pageIndex = pageIndex < DEFAULT_PAGE_INDEX ? DEFAULT_PAGE_INDEX : pageIndex;
            pageSize = pageSize < pageIndex ? DEFAULT_PAGE_SIZE : pageSize;
            var query = biblioEntities.Suggestions
            .Include(x => x.User)
            .Where(x => true);
            
                query = query.Where
                (
                    x =>
                    (x.Subject.ToLower().Contains(value) || value.Contains(x.Subject)) ||
                    (x.Message.ToLower().Contains(value) || value.Contains(x.Message)) ||
                    (x.User.FullName.ToLower().Contains(value) || value.Contains(x.User.FullName)) ||
                    (x.User.Account.ToLower().Contains(value) || value.Contains(x.User.Account))
                );

            if (userId.HasValue)
                query = query.Where(x => x.UserId == userId.Value);

            if (orderBy != null)
                return
                (
                    await query.OrderBy(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArrayAsync()
                ).Select
                (
                    x => GetSuggestionModel(x, mediaFolderPath)
                ).ToArray();
            else if (orderByDescending != null)
                return
                (
                    await query.OrderByDescending(orderByDescending).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArrayAsync()
                ).Select
                (
                    x => GetSuggestionModel(x, mediaFolderPath)
                ).ToArray();
            else
                return
                (
                    await query.OrderByDescending(x=> x.Date).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArrayAsync()
                ).Select
                (
                    x => GetSuggestionModel(x, mediaFolderPath)
                ).ToArray();
        }

        private SuggestionModel GetSuggestionModel(Suggestion suggestion, string mediaFolderPath)
        {
            return suggestion != null ? new SuggestionModel
            (
                suggestion,
                mediaFolderPath,
                mediaFolderPath
            ) : null;
        }


        public async Task<int> UnReadCountAsync()
        {
            return await biblioEntities.Suggestions.Where
                (x => x.IsReaded == false).CountAsync();          
        }


        public async Task<IEnumerable<SuggestionModel>> TopSuggestions(int limit, string mediaFolderPath)
        {
            return 
            (
                await biblioEntities.Suggestions
                .Include(x => x.User)
                .Where(x => x.IsReaded == false)      
                .OrderByDescending(x => x.Date).Take(limit).ToArrayAsync()
            )
            .Select
            (
                x => GetSuggestionModel(x, mediaFolderPath)
            ).ToArray();
        }

        public async Task<SuggestionModel> GetAsync(int id, string mediaFolderPath)
        {
            var suggestion = await biblioEntities.Suggestions.Include(x=> x.User)
                .SingleOrDefaultAsync(x=> x.Id == id);
            if (suggestion != null)
            {

                var mediaBasePath = Path.Combine(env.WebRootPath, mediaFolderPath.Replace("~/", string.Empty));
                return GetSuggestionModel(suggestion, mediaFolderPath);
            }
            return null;            
        }

        public async Task<SuggestionModel> GetAndSetAsReadAsync(int id, string mediaFolderPath)
        {
            var suggestion = await biblioEntities.Suggestions.Include(x => x.User)
                .SingleOrDefaultAsync(x => x.Id == id);
            if (suggestion != null)
            {
                suggestion.IsReaded = true;
                await biblioEntities.SaveChangesAsync();
                var mediaBasePath = Path.Combine(env.WebRootPath, mediaFolderPath.Replace("~/", string.Empty));
                return GetSuggestionModel(suggestion, mediaFolderPath);
            }
            return null;
        }



        public async Task RemoveAsync(int id, string mediaFolderPath)
        {
            var suggestion = await biblioEntities.Suggestions.FindAsync(id);
            if (suggestion != null)
            {
                biblioEntities.Suggestions.Remove(suggestion);
                await biblioEntities.SaveChangesAsync();
                if(!string.IsNullOrEmpty(suggestion.File))
                {
                    var mediaBasePath = Path.Combine(env.WebRootPath, mediaFolderPath.Replace("~/", string.Empty));
                    OssFile.DeleteFile(suggestion.File, mediaBasePath);
                    OssFile.DeleteFile(suggestion.File, mediaBasePath);
                }
            }
        }

        public async Task<SuggestionModel> AddAsync(SuggestionModel suggestionModel,
            string mediaFolderPath, string prefixSuggestionFileName)
        {
            var mediaAbsoluteBasePath = Path.Combine(env.WebRootPath, mediaFolderPath.Replace("~/", string.Empty));
            string newFileName = null;
            try
            {
                if (suggestionModel == null)
                    throw new ArgumentNullException("suggestionModel");

        
                if (OssFile.HasFile(suggestionModel.FileUploaded))
                {
                    newFileName = await OssFile.SaveFile(suggestionModel.FileUploaded, prefixSuggestionFileName, mediaAbsoluteBasePath); ;
                }
                else
                {
                    if (!string.IsNullOrEmpty(suggestionModel.FileUploadedName) &&
                    File.Exists(Path.Combine(mediaAbsoluteBasePath, suggestionModel.FileUploadedName)))
                    {
                        newFileName = OssFile.GetNewFileName(suggestionModel.FileUploadedName, prefixSuggestionFileName);
                        File.Move
                        (
                            Path.Combine(mediaAbsoluteBasePath, suggestionModel.FileUploadedName),
                            Path.Combine(mediaAbsoluteBasePath, newFileName)
                        );
                    }
                }

                Suggestion newSuggestion = new Suggestion
                (
                    suggestionModel.Id,
                    suggestionModel.Subject,
                    suggestionModel.Message,
                    newFileName,
                    suggestionModel.IsRead,
                    suggestionModel.IsSolved,
                    DateTime.UtcNow,
                    suggestionModel.UserModel.Id,
                    null
                );

                biblioEntities.Suggestions.Add(newSuggestion);
                await biblioEntities.SaveChangesAsync();
                return new SuggestionModel(newSuggestion, mediaFolderPath, mediaFolderPath);
            }
            catch (Exception ex)
            {
                if (OssFile.HasFile(suggestionModel.FileUploaded) && !string.IsNullOrEmpty(newFileName))
                    OssFile.DeleteFile(newFileName, mediaAbsoluteBasePath);
                throw ex;
            }
        }

        public async Task<SuggestionModel> SetAsync(SuggestionModel suggestionModel, 
            string mediaFolderPath, string mediaFolderTmpPath, string prefixSuggestionImageName, string prefixSuggestionFileName)
        {
            string newFileName = null;
            var mediaAbsoluteBasePath = Path.Combine(env.WebRootPath, mediaFolderPath?.Replace("~/", string.Empty));
            try
            {
                if (suggestionModel == null)
                    throw new ArgumentNullException("suggestionModel");

                var currentSuggestion = await biblioEntities.Suggestions.FindAsync(suggestionModel.Id);
                if (currentSuggestion == null)
                    throw new KeyNotFoundException("Suggestion");

                bool deleteCurrentFile = false;
                string currentSuggestionFile = currentSuggestion.File;
                

                if (OssFile.HasFile(suggestionModel.FileUploaded))
                {
                    newFileName = await OssFile.SaveFile(suggestionModel.FileUploaded, prefixSuggestionFileName, mediaAbsoluteBasePath); ;
                    deleteCurrentFile = true;
                }
                else if (!string.IsNullOrEmpty(suggestionModel.FileUploadedName) &&
                    File.Exists(Path.Combine(mediaAbsoluteBasePath, suggestionModel.FileUploadedName)))
                {
                    newFileName = OssFile.GetNewFileName(suggestionModel.FileUploadedName, prefixSuggestionFileName);
                    File.Move
                    (
                        Path.Combine(mediaAbsoluteBasePath, suggestionModel.FileUploadedName),
                        Path.Combine(mediaAbsoluteBasePath, newFileName)
                    );
                    deleteCurrentFile = true;
                }
                else
                {
                    newFileName = currentSuggestion.File;
                }

                Suggestion newSuggestion = new Suggestion
               (
                   suggestionModel.Id,
                   suggestionModel.Subject,
                   suggestionModel.Message,
                   newFileName,
                   suggestionModel.IsRead,
                   suggestionModel.IsSolved,
                   currentSuggestion.Date,
                   currentSuggestion.UserId,
                   null
               );

                biblioEntities.Entry(currentSuggestion).CurrentValues.SetValues(newSuggestion);
                await biblioEntities.SaveChangesAsync();

               
                if(deleteCurrentFile)
                    OssFile.DeleteFile(currentSuggestionFile, mediaAbsoluteBasePath);

                return new SuggestionModel(newSuggestion, mediaFolderPath, mediaFolderPath);
            }
            catch (Exception ex)
            {
                if (OssFile.HasFile(suggestionModel.FileUploaded) && !string.IsNullOrEmpty(newFileName))
                    OssFile.DeleteFile(newFileName, mediaAbsoluteBasePath);
                throw ex;
            }
        }
    }
}
