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
using System.Threading;
using System.Threading.Tasks;

namespace BiblioIUC.Logics
{

    public class DocumentLogic : IDocumentLogic
    {
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_PAGE_SIZE = 10;

        private readonly BiblioEntities biblioEntities;
        private readonly IWebHostEnvironment env;

        public DocumentLogic(BiblioEntities biblioEntities, IWebHostEnvironment env)
        {
            this.biblioEntities = biblioEntities;
            this.env = env;
        }

        public async Task<string> GenerateCodeAsync(string codePrefix)
        {
          
            string code = Generate(codePrefix);
            var rdv = await biblioEntities.Documents.FirstOrDefaultAsync(x => x.Code == code);
            while (rdv != null)
            {
                Thread.Sleep(500);
                code = Generate(codePrefix);
                rdv = await biblioEntities.Documents.FirstOrDefaultAsync(x => x.Code == code);
            }
            return code;
        }

        private string Generate(string codePrefix)
        {
            int min = 100000;
            int max = 999999;
            var r = new Random();
            long n = r.Next(min, max + 1);
            return codePrefix + DateTime.UtcNow.Year + DateTime.UtcNow.Month + n.ToString();
        }

        public async Task<IEnumerable<DocumentModel>> FindAsync(int[] documentIds, string value, string mediaFolderPath,
            Expression<Func<Document, object>> orderBy,
            bool withDisabled = false,  int pageIndex = DEFAULT_PAGE_INDEX, 
            int pageSize = DEFAULT_PAGE_SIZE)
        {
            value = value?.ToLower() ?? string.Empty;
            pageIndex = pageIndex < DEFAULT_PAGE_INDEX ? DEFAULT_PAGE_INDEX : pageIndex;
            pageSize = pageSize < pageIndex ? DEFAULT_PAGE_SIZE : pageSize;
            var query = biblioEntities.Documents
            .Include(x => x.Category)
            .Where(x => true);
            if (documentIds !=  null && documentIds.Count() > 0)
            {
                query = query.Where(x => documentIds.Any(y => y == x.Id));
            }
            else
            {
                query = query.Where
                (
                    x =>
                    (x.Title.ToLower().Contains(value) || value.Contains(x.Title)) ||
                    (x.Subtitle.ToLower().Contains(value) || value.Contains(x.Subtitle)) ||
                    (x.Description.ToLower().Contains(value) || value.Contains(x.Description)) ||
                    (x.Publisher.ToLower().Contains(value) || value.Contains(x.Publisher)) ||
                    (x.Contributors.ToLower().Contains(value) || value.Contains(x.Contributors)) ||
                    (x.Language.ToLower().Contains(value) || value.Contains(x.Language))
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
                x => GetDocumentModel(x, mediaFolderPath)
            ).ToArray();
        }

        public void UpdateMetaData(string mediaFolderPath, DocumentModel documentModel)
        {
            //TODO : update metadata
        }

        private DocumentModel GetDocumentModel(Document document, string mediaFolderPath)
        {
            return document != null ? new DocumentModel
            (
                document,
                mediaFolderPath,
                mediaFolderPath
            ) : null;
        }

        public async Task<DocumentModel> GetAsync(int id, string mediaFolderPath)
        {
            var document = await biblioEntities.Documents.Include(x=> x.Category)
                .SingleOrDefaultAsync(x=> x.Id == id);
            if (document != null)
                return GetDocumentModel(document, mediaFolderPath);
            return null;            
        }

        public async Task RemoveAsync(int id, string mediaFolderPath)
        {
            var document = await biblioEntities.Documents.FindAsync(id);
            if (document != null)
            {
                biblioEntities.Documents.Remove(document);
                await biblioEntities.SaveChangesAsync();
                if(!string.IsNullOrEmpty(document.Image))
                {
                    var mediaBasePath = Path.Combine(env.WebRootPath, mediaFolderPath.Replace("~/", string.Empty));
                    Tools.OssFile.DeleteFile(document.Image, mediaBasePath);
                    Tools.OssFile.DeleteFile(document.File, mediaBasePath);
                }
            }
        }

        public async Task<bool> CodeAlreadyExistsAsync(string name, int id)
        {
            try
            {
                return await biblioEntities.Documents.SingleOrDefaultAsync
                (
                    x =>
                    x.Code.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                    x.Id != id
                ) != null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<DocumentModel> AddAsync(DocumentModel documentModel,
            string mediaFolderPath, string PrefixDocumentImageName, string PrefixDocumentFileName)
        {
            var mediaBasePath = Path.Combine(env.WebRootPath, mediaFolderPath.Replace("~/", string.Empty));
            string newImageName = null;
            string newFileName = null;
            try
            {
                if (documentModel == null)
                    throw new ArgumentNullException("documentModel");

                if (Tools.OssFile.HasImage(documentModel.ImageUploaded))
                {
                    newImageName = Tools.OssFile.SaveImage(documentModel.ImageUploaded, 400, 600, 100 * 1024, 300 * 1024, PrefixDocumentImageName, mediaBasePath); ;
                }

                if (Tools.OssFile.HasFile(documentModel.FileUploaded))
                {
                    newFileName = await Tools.OssFile.SaveFile(documentModel.FileUploaded, PrefixDocumentFileName, mediaBasePath); ;
                }

                Document newDocument = new Document
                (
                    documentModel.Id,
                    documentModel.Code,
                    documentModel.Title,
                    documentModel.Subtitle,
                    documentModel.Authors,
                    documentModel.Description,
                    documentModel.Language,
                    documentModel.PublishDate,
                    documentModel.Publisher,
                    documentModel.NumberOfPages,
                    documentModel.Contributors,
                    documentModel.CategoryId,
                    null,
                    newFileName,
                    newImageName,
                    DateTime.UtcNow,
                    (short)documentModel.Status
                );

                biblioEntities.Documents.Add(newDocument);
                await biblioEntities.SaveChangesAsync();
                return new DocumentModel(newDocument, mediaFolderPath, mediaFolderPath);
            }
            catch (Exception ex)
            {
                if (Tools.OssFile.HasImage(documentModel.ImageUploaded) && !string.IsNullOrEmpty(newImageName))
                    Tools.OssFile.DeleteFile(newImageName, mediaBasePath);
                if (Tools.OssFile.HasFile(documentModel.FileUploaded) && !string.IsNullOrEmpty(newFileName))
                    Tools.OssFile.DeleteFile(newFileName, mediaBasePath);
                throw ex;
            }
        }

        public async Task<DocumentModel> SetAsync(DocumentModel documentModel, 
            string mediaFolderPath, string PrefixDocumentImageName, string PrefixDocumentFileName)
        {
            string newImageName = null;
            string newFileName = null;
            var mediaBasePath = Path.Combine(env.WebRootPath, mediaFolderPath?.Replace("~/", string.Empty));
            try
            {
                if (documentModel == null)
                    throw new ArgumentNullException("documentModel");

                var currentDocument = await biblioEntities.Documents.FindAsync(documentModel.Id);
                if (currentDocument == null)
                    throw new KeyNotFoundException("Document");


                if (Tools.OssFile.HasImage(documentModel.ImageUploaded))
                {
                    newImageName = Tools.OssFile.SaveImage(documentModel.ImageUploaded, 400, 600, 100 * 1024, 300 * 1024, PrefixDocumentImageName, mediaBasePath); ;
                    Tools.OssFile.DeleteFile(currentDocument.Image, mediaBasePath);
                }
                else if (!string.IsNullOrEmpty(currentDocument.Image) && documentModel.DeleteImage)
                {
                    Tools.OssFile.DeleteFile(currentDocument.Image, mediaBasePath);
                }
                else
                {
                    newImageName = currentDocument.Image;
                }


                if (Tools.OssFile.HasFile(documentModel.FileUploaded))
                {
                    newFileName = await Tools.OssFile.SaveFile(documentModel.FileUploaded, PrefixDocumentFileName, mediaBasePath); ;
                    Tools.OssFile.DeleteFile(currentDocument.File, mediaBasePath);
                }
                else
                {
                    newFileName = currentDocument.File;
                }


                Document newDocument = new Document
               (
                   documentModel.Id,
                   documentModel.Code,
                   documentModel.Title,
                   documentModel.Subtitle,
                   documentModel.Authors,
                   documentModel.Description,
                   documentModel.Language,
                   documentModel.PublishDate,
                   documentModel.Publisher,
                   documentModel.NumberOfPages,
                   documentModel.Contributors,
                   documentModel.CategoryId,
                   null,
                   newFileName,
                   newImageName,
                   currentDocument.CreateDate,
                   (short)documentModel.Status
               );

                biblioEntities.Entry(currentDocument).CurrentValues.SetValues(newDocument);
                await biblioEntities.SaveChangesAsync();
                return new DocumentModel(newDocument, mediaFolderPath, mediaFolderPath);
            }
            catch (Exception ex)
            {
                if(Tools.OssFile.HasImage(documentModel.ImageUploaded) && !string.IsNullOrEmpty(newImageName))
                    Tools.OssFile.DeleteFile(newImageName, mediaBasePath);
                if (Tools.OssFile.HasFile(documentModel.FileUploaded) && !string.IsNullOrEmpty(newFileName))
                    Tools.OssFile.DeleteFile(newFileName, mediaBasePath);
                throw ex;
            }
        }
    }
}
