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
        Task<IEnumerable<DocumentModel>> FindAsync(string value, string mediaFolderPath,
            Expression<Func<Document, object>> orderBy, bool withDisabled = false, int pageIndex = 1, int pageSize = 10);
        Task<DocumentModel> GetAsync(int id, string mediaFolderPath);
        Task<DocumentModel> SetAsync(DocumentModel documentModel, 
            string mediaFolderPath, string PrefixDocumentImageName, string PrefixDocumentFileName);
        Task<bool> IsbnAlreadyExistsAsync(string name, int id);
        Task<DocumentModel> AddAsync(DocumentModel documentModel, 
            string mediaFolderPath, string PrefixDocumentImageName, string PrefixDocumentFileName);
        Task RemoveAsync(int id, string mediaFolderPath);
    }
}