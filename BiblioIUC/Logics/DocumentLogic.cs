using BiblioIUC.Entities;
using BiblioIUC.Localize;
using BiblioIUC.Logics.Interfaces;
using BiblioIUC.Models;
using iTextSharp.text.pdf;
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
            Expression<Func<Document, object>> orderByDescending,
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
                    (x.Code.ToLower().Contains(value) || value.Contains(x.Code)) ||
                    (x.Title.ToLower().Contains(value) || value.Contains(x.Title)) ||
                    (x.Subtitle.ToLower().Contains(value) || value.Contains(x.Subtitle)) ||
                    (x.Description.ToLower().Contains(value) || value.Contains(x.Description)) ||
                    (x.Authors.ToLower().Contains(value) || value.Contains(x.Authors)) ||
                    (x.Publisher.ToLower().Contains(value) || value.Contains(x.Publisher)) ||
                    (x.Contributors.ToLower().Contains(value) || value.Contains(x.Contributors)) ||
                    (x.Language.ToLower().Contains(value) || value.Contains(x.Language))
                );
            }

            if (!withDisabled)
                query = query.Where(x => x.Status == (short)StatusOptions.Actived);

            if (orderBy != null)
                return
                (
                    await query.OrderBy(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArrayAsync()
                ).Select
                (
                    x => GetDocumentModel(x, mediaFolderPath, null)
                ).ToArray();
            else if (orderByDescending != null)
                return
                (
                    await query.OrderByDescending(orderByDescending).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArrayAsync()
                ).Select
                (
                    x => GetDocumentModel(x, mediaFolderPath, null)
                ).ToArray();
            else
                return
                (
                    await query.OrderBy(x=> x.Title).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArrayAsync()
                ).Select
                (
                    x => GetDocumentModel(x, mediaFolderPath, null)
                ).ToArray();
        }

        public async Task<DocumentModel> ExtractMetadata(DocumentModel documentModel,
         string PrefixDocumentTmpFileName, string mediaFolderTmpPath, string libGostScriptPath, int userId)
        {
            var mediaBaseTmpPath = Path.Combine(env.WebRootPath, mediaFolderTmpPath.Replace("~/", string.Empty).Replace("/", @"\"));
            var libBaseGostScriptPath = Path.Combine(env.ContentRootPath, libGostScriptPath.Replace("~/", string.Empty));
            var prefixTmpThumb = "tmp_" + userId + "_";

            Tools.OssFile.DeleteFileInFolder(prefixTmpThumb, mediaBaseTmpPath);

            string newFileName = string.Empty;
            if (Tools.OssFile.HasFile(documentModel.FileUploaded))
            {
                newFileName = await Tools.OssFile.SaveFile(documentModel.FileUploaded, prefixTmpThumb, mediaBaseTmpPath); ;
                documentModel.FileUploadedName = documentModel.FileUploaded.FileName;
            }

            string coverFileName = prefixTmpThumb + Guid.NewGuid().ToString().ToLower() + ".png";

            Tools.OssFile.ConvertPdfToImage
            (
                libBaseGostScriptPath,
                Path.Combine(mediaBaseTmpPath, newFileName),
                1,
                Path.Combine(mediaBaseTmpPath, coverFileName),
                400,
                600
            );

            PdfReader reader = new PdfReader(Path.Combine(mediaBaseTmpPath, newFileName));
            documentModel.Code = reader.Info["ISBN"]?.ToString() ?? documentModel.Code;
            documentModel.Title = reader.Info["Title"]?.ToString();
            documentModel.Authors = reader.Info["Author"]?.ToString();
            documentModel.Description = reader.Info["Subject"]?.ToString() + (!string.IsNullOrEmpty(reader.Info["Keywords"]?.ToString()) ? " " + reader.Info["Keywords"]?.ToString() : null);
            documentModel.Language = reader.Info["Language"]?.ToString();
            documentModel.Publisher = reader.Info["Creator"]?.ToString();
            documentModel.Contributors = reader.Info["Contributors"]?.ToString();
            documentModel.NumberOfPages = reader.NumberOfPages;
            documentModel.FileUploadedTmpFileName = newFileName;
            documentModel.ImageUploadedTmpFileName = coverFileName;
            documentModel.ImageLink = $"{mediaFolderTmpPath}/{coverFileName}";
            reader.Close();
            return documentModel;
        }

        public void UpdateMetaData(string mediaFolderPath, DocumentModel documentModel)
        {
            //TODO : update metadata
        }

        private DocumentModel GetDocumentModel(Document document, string mediaFolderPath, string mediaBasePath)
        {
            return document != null ? new DocumentModel
            (
                document,
                mediaFolderPath,
                mediaFolderPath,
                mediaBasePath
            ) : null;
        }

        public async Task<DocumentModel> GetAsync(int id, string mediaFolderPath)
        {
            var document = await biblioEntities.Documents.Include(x=> x.Category)
                .SingleOrDefaultAsync(x=> x.Id == id);
            if (document != null)
            {
                var mediaBasePath = Path.Combine(env.WebRootPath, mediaFolderPath.Replace("~/", string.Empty));
                return GetDocumentModel(document, mediaFolderPath, mediaBasePath);
            }
            return null;            
        }

        public async Task<DocumentModel> GetAsync(string code, RoleOptions role, string mediaFolderPath)
        {
            var query = biblioEntities.Documents.Include(x => x.Category).Where(x => x.Code == code);
            if (role != RoleOptions.Admin)
                query = query.Where(x => x.Status == (short)StatusOptions.Actived);

            var document = await query.SingleOrDefaultAsync();
            if (document != null)
            {
                var mediaBasePath = Path.Combine(env.WebRootPath, mediaFolderPath.Replace("~/", string.Empty));
                return GetDocumentModel(document, mediaFolderPath, mediaBasePath);
            }
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
            string mediaFolderPath, string mediaFolderTmpPath, string prefixDocumentImageName, string prefixDocumentFileName)
        {
            var mediaAbsoluteBasePath = Path.Combine(env.WebRootPath, mediaFolderPath.Replace("~/", string.Empty));
            var mediaAbsoluteBaseTmpPath = Path.Combine(env.WebRootPath, mediaFolderTmpPath.Replace("~/", string.Empty).Replace("/", @"\"));
            string newImageName = null;
            string newFileName = null;
            try
            {
                if (documentModel == null)
                    throw new ArgumentNullException("documentModel");

                if (Tools.OssFile.HasImage(documentModel.ImageUploaded))
                {
                    newImageName = Tools.OssFile.SaveImage(documentModel.ImageUploaded, 400, 600, 100 * 1024, 300 * 1024, prefixDocumentImageName, mediaAbsoluteBasePath); ;
                }
                else
                {
                    if (!documentModel.DeleteImage)
                    {
                        if (!string.IsNullOrEmpty(documentModel.ImageUploadedTmpFileName) &&
                        File.Exists(Path.Combine(mediaAbsoluteBaseTmpPath, documentModel.ImageUploadedTmpFileName)))
                        {
                            newImageName = Tools.OssFile.GetNewFileName(documentModel.ImageUploadedTmpFileName, prefixDocumentImageName);
                            File.Move
                            (
                                Path.Combine(mediaAbsoluteBaseTmpPath, documentModel.ImageUploadedTmpFileName),
                                Path.Combine(mediaAbsoluteBasePath, newImageName)
                            );
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(documentModel.ImageUploadedTmpFileName) &&
                        File.Exists(Path.Combine(mediaAbsoluteBaseTmpPath, documentModel.ImageUploadedTmpFileName)))
                        {
                            File.Delete
                            (
                                Path.Combine(mediaAbsoluteBaseTmpPath, documentModel.ImageUploadedTmpFileName)
                            );
                        }
                    }
                }

                if (Tools.OssFile.HasFile(documentModel.FileUploaded))
                {
                    newFileName = await Tools.OssFile.SaveFile(documentModel.FileUploaded, prefixDocumentFileName, mediaAbsoluteBasePath); ;
                }
                else
                {
                    if (!string.IsNullOrEmpty(documentModel.FileUploadedTmpFileName) &&
                    File.Exists(Path.Combine(mediaAbsoluteBaseTmpPath, documentModel.FileUploadedTmpFileName)))
                    {
                        newFileName = Tools.OssFile.GetNewFileName(documentModel.FileUploadedTmpFileName, prefixDocumentFileName);
                        File.Move
                        (
                            Path.Combine(mediaAbsoluteBaseTmpPath, documentModel.FileUploadedTmpFileName),
                            Path.Combine(mediaAbsoluteBasePath, newFileName)
                        );
                    }
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
                return new DocumentModel(newDocument, mediaFolderPath, mediaFolderPath, null);
            }
            catch (Exception ex)
            {
                if (Tools.OssFile.HasImage(documentModel.ImageUploaded) && !string.IsNullOrEmpty(newImageName))
                    Tools.OssFile.DeleteFile(newImageName, mediaAbsoluteBasePath);
                if (Tools.OssFile.HasFile(documentModel.FileUploaded) && !string.IsNullOrEmpty(newFileName))
                    Tools.OssFile.DeleteFile(newFileName, mediaAbsoluteBasePath);
                throw ex;
            }
        }

        public async Task<DocumentModel> SetAsync(DocumentModel documentModel, 
            string mediaFolderPath, string mediaFolderTmpPath, string prefixDocumentImageName, string prefixDocumentFileName)
        {
            string newImageName = null;
            string newFileName = null;
            var mediaAbsoluteBasePath = Path.Combine(env.WebRootPath, mediaFolderPath?.Replace("~/", string.Empty));
            var mediaAbsoluteBaseTmpPath = Path.Combine(env.WebRootPath, mediaFolderTmpPath.Replace("~/", string.Empty).Replace("/", @"\"));
            try
            {
                if (documentModel == null)
                    throw new ArgumentNullException("documentModel");

                var currentDocument = await biblioEntities.Documents.FindAsync(documentModel.Id);
                if (currentDocument == null)
                    throw new KeyNotFoundException("Document");

                bool deleteCurrentIamge = false, deleteCurrentFile = false;
                string currentDocumentImage = currentDocument.Image;
                string currentDocumentFile = currentDocument.File;
                if (Tools.OssFile.HasImage(documentModel.ImageUploaded))
                {
                    newImageName = Tools.OssFile.SaveImage(documentModel.ImageUploaded, 400, 600, 100 * 1024, 300 * 1024, prefixDocumentImageName, mediaAbsoluteBasePath); ;
                    deleteCurrentIamge = true;
                }
                else if (string.IsNullOrEmpty(documentModel.ImageUploadedTmpFileName) && !string.IsNullOrEmpty(currentDocument.Image) && documentModel.DeleteImage)
                {
                    deleteCurrentIamge = true;
                }
                else if (!documentModel.DeleteImage && !string.IsNullOrEmpty(documentModel.ImageUploadedTmpFileName) &&
                    File.Exists(Path.Combine(mediaAbsoluteBaseTmpPath, documentModel.ImageUploadedTmpFileName)))
                {
                    newImageName = Tools.OssFile.GetNewFileName(documentModel.ImageUploadedTmpFileName, prefixDocumentImageName);
                    File.Move
                    (
                        Path.Combine(mediaAbsoluteBaseTmpPath, documentModel.ImageUploadedTmpFileName),
                        Path.Combine(mediaAbsoluteBasePath, newImageName)
                    );
                    deleteCurrentIamge = true;
                }
                else if (documentModel.DeleteImage && !string.IsNullOrEmpty(documentModel.ImageUploadedTmpFileName) &&
                    File.Exists(Path.Combine(mediaAbsoluteBaseTmpPath, documentModel.ImageUploadedTmpFileName)))
                {
                    File.Delete
                    (
                        Path.Combine(mediaAbsoluteBaseTmpPath, documentModel.ImageUploadedTmpFileName)
                    );
                    newImageName = currentDocument.Image;
                }
                else
                {
                    newImageName = currentDocument.Image;
                }


                if (Tools.OssFile.HasFile(documentModel.FileUploaded))
                {
                    newFileName = await Tools.OssFile.SaveFile(documentModel.FileUploaded, prefixDocumentFileName, mediaAbsoluteBasePath); ;
                    deleteCurrentFile = true;
                }
                else if (!string.IsNullOrEmpty(documentModel.FileUploadedTmpFileName) &&
                    File.Exists(Path.Combine(mediaAbsoluteBaseTmpPath, documentModel.FileUploadedTmpFileName)))
                {
                    newFileName = Tools.OssFile.GetNewFileName(documentModel.FileUploadedTmpFileName, prefixDocumentFileName);
                    File.Move
                    (
                        Path.Combine(mediaAbsoluteBaseTmpPath, documentModel.FileUploadedTmpFileName),
                        Path.Combine(mediaAbsoluteBasePath, newFileName)
                    );
                    deleteCurrentFile = true;
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

                if(deleteCurrentIamge)
                    Tools.OssFile.DeleteFile(currentDocumentImage, mediaAbsoluteBasePath);
                if(deleteCurrentFile)
                    Tools.OssFile.DeleteFile(currentDocumentFile, mediaAbsoluteBasePath);

                return new DocumentModel(newDocument, mediaFolderPath, mediaFolderPath, null);
            }
            catch (Exception ex)
            {
                if(Tools.OssFile.HasImage(documentModel.ImageUploaded) && !string.IsNullOrEmpty(newImageName))
                    Tools.OssFile.DeleteFile(newImageName, mediaAbsoluteBasePath);
                if (Tools.OssFile.HasFile(documentModel.FileUploaded) && !string.IsNullOrEmpty(newFileName))
                    Tools.OssFile.DeleteFile(newFileName, mediaAbsoluteBasePath);
                throw ex;
            }
        }
    }
}
