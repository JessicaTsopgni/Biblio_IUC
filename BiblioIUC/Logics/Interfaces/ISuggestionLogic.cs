using BiblioIUC.Entities;
using BiblioIUC.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BiblioIUC.Logics.Interfaces
{
    public interface ISuggestionLogic
    {
        Task<IEnumerable<SuggestionModel>> FindAsync(string value, int? userId, string mediaFolderPath,
            Expression<Func<Suggestion, object>> orderBy, Expression<Func<Suggestion, object>> orderByDescending,
     int pageIndex = 1, int pageSize = 10);
        Task<SuggestionModel> GetAsync(int id, string mediaFolderPath);
        Task<SuggestionModel> SetAsync(SuggestionModel suggestionModel, 
            string mediaFolderPath, string mediaFolderTmpPath, string PrefixSuggestionImageName, string PrefixSuggestionFileName);
        Task<SuggestionModel> AddAsync(SuggestionModel suggestionModel, 
            string mediaFolderPath,  string PrefixSuggestionFileName);
        Task RemoveAsync(int id, string mediaFolderPath);
        Task<int> UnReadCountAsync();
        Task<IEnumerable<SuggestionModel>> TopSuggestions(int limit, string mediaFolderPath);
    }
}