using BiblioIUC.Entities;
using BiblioIUC.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BiblioIUC.Logics.Interfaces
{
    public interface IDocumentLogic
    {
        Task<IEnumerable<DocumentModel>> FindAsync(int[] documentIds, string value, string mediaFolderPath,
            Expression<Func<Document, object>> orderBy, Expression<Func<Document, object>> orderByDescending,
 bool withDisabled = false, int pageIndex = 1, int pageSize = 10);
        Task<DocumentModel> GetAsync(int id, string mediaFolderPath, int userId);
        Task<DocumentModel> SetAsync(DocumentModel documentModel, 
            string mediaFolderPath, string mediaFolderTmpPath, string PrefixDocumentImageName, string PrefixDocumentFileName);
        Task<bool> CodeAlreadyExistsAsync(string name, int id);
        Task<DocumentModel> AddAsync(DocumentModel documentModel, 
            string mediaFolderPath, string mediaFolderTmpPath, string PrefixDocumentImageName, string PrefixDocumentFileName);
        Task RemoveAsync(int id, string mediaFolderPath);
        Task<string> GenerateCodeAsync(string codePrefix);
        Task UpdateMetaData(string mediaFolderPath, string prefixDocumentFileName, DocumentModel documentModel);
        Task<DocumentModel> GetAsync(string code, RoleOptions role, string mediaFolderPath, int userId);
        Task<DocumentModel> ExtractMetadata(DocumentModel documentModel, string PrefixDocumentTmpFileName, string mediaFolderTmpPath, string libGostScriptPath, int userId);
        Task SaveLastReadAsync(string code, int userId, int? lastPageNumber);
        Task IncrementCountReadAsync(string code, int userId);
    }
}