﻿using BiblioIUC.Entities;
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
                    x => GetDocumentModel(x, mediaFolderPath, null, 0)
                ).ToArray();
            else if (orderByDescending != null)
                return
                (
                    await query.OrderByDescending(orderByDescending).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArrayAsync()
                ).Select
                (
                    x => GetDocumentModel(x, mediaFolderPath, null, 0)
                ).ToArray();
            else
                return
                (
                    await query.OrderBy(x=> x.Title).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArrayAsync()
                ).Select
                (
                    x => GetDocumentModel(x, mediaFolderPath, null, 0)
                ).ToArray();
        }

        public async Task<DocumentModel> ExtractMetadata(DocumentModel documentModel,
         string PrefixDocumentTmpFileName, string mediaFolderTmpPath, string libGostScriptPath, int userId)
        {
            var mediaBaseTmpPath = Path.Combine(env.WebRootPath, mediaFolderTmpPath.Replace("~/", string.Empty).Replace("/", @"\"));
            var libBaseGostScriptPath = Path.Combine(env.ContentRootPath, libGostScriptPath.Replace("~/", string.Empty));
            var prefixTmpThumb = "tmp_" + userId + "_";

            OssFile.DeleteFileInFolder(prefixTmpThumb, mediaBaseTmpPath);

            string newFileName = string.Empty;
            if (OssFile.HasFile(documentModel.FileUploaded))
            {
                newFileName = await OssFile.SaveFile(documentModel.FileUploaded, prefixTmpThumb, mediaBaseTmpPath); ;
                documentModel.FileUploadedName = documentModel.FileUploaded.FileName;
            }

            string coverFileName = prefixTmpThumb + Guid.NewGuid().ToString().ToLower() + ".png";

            OssFile.ConvertPdfToImage
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
            documentModel.CategoryId = int.Parse(reader.Info["CategoryId"]?.ToString() ?? "0");
            documentModel.Contributors = reader.Info["Contributors"]?.ToString();
            documentModel.NumberOfPages = reader.NumberOfPages;
            documentModel.FileUploadedTmpFileName = newFileName;
            documentModel.ImageUploadedTmpFileName = coverFileName;
            documentModel.ImageLink = $"{mediaFolderTmpPath}/{coverFileName}";
            reader.Close();
            return documentModel;
        }

        public async Task UpdateMetaData(string mediaFolderPath, string prefixDocumentFileName,  DocumentModel documentModel)
        {
            try
            {
                var document = await biblioEntities.Documents
                    .SingleOrDefaultAsync(x => x.Id == documentModel.Id);

                if (document != null)
                {
                    string inputFile = Path.Combine(env.WebRootPath, documentModel.FileLink.Replace("~/", string.Empty).Replace("/", @"\"));

                    string mediaBasePath = Path.Combine(env.WebRootPath, mediaFolderPath.Replace("~/", string.Empty));
                    
                    string newFileName = OssFile.GetNewFileName(inputFile, prefixDocumentFileName);

                    string outputFile = Path.Combine(mediaBasePath, newFileName);

                    using (FileStream fs = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        PdfReader reader = new PdfReader(inputFile);
                        PdfStamper stamper = new PdfStamper(reader, fs);

                        Hashtable info = reader.Info;
                        if(info.ContainsKey("ISBN"))
                            info.Remove("ISBN");
                        info.Add("ISBN", documentModel.Code);
                        if (info.ContainsKey("Title")) 
                            info.Remove("Title");
                        info.Add("Title", documentModel.Title);
                        if (info.ContainsKey("Subtitle"))
                            info.Remove("Subtitle");
                        info.Add("Subtitle", documentModel.Subtitle);
                        if (info.ContainsKey("Subject"))
                            info.Remove("Subject");
                        info.Add("Subject", documentModel.Description);
                        if (info.ContainsKey("Creator"))
                            info.Remove("Creator");
                        info.Add("Creator", documentModel.Publisher);
                        if (documentModel.PublishDate.HasValue)
                        {
                            if (info.ContainsKey("PublishDate"))
                                info.Remove("PublishDate");
                            info.Add("PublishDate", documentModel.PublishDate.Value.ToString("yyyy-MM-dd"));
                        }
                        if (info.ContainsKey("Author"))
                            info.Remove("Author");
                        info.Add("Author", documentModel.Authors);
                        if (info.ContainsKey("Contributors"))
                            info.Remove("Contributors");
                        info.Add("Contributors", documentModel.Contributors);
                        if (info.ContainsKey("Language"))
                            info.Remove("Language");
                        info.Add("Language", documentModel.Language);
                        if (info.ContainsKey("CategoryId"))
                            info.Remove("CategoryId");
                        info.Add("CategoryId", documentModel.CategoryId.ToString());
                        if (info.ContainsKey("CategoryName"))
                            info.Remove("CategoryName");
                        info.Add("CategoryName", documentModel.CategoryName);
                        stamper.MoreInfo = info;
                        stamper.Close();
                        reader.Close();
                    }
                    document.File = newFileName;
                    await biblioEntities.SaveChangesAsync();
                    try
                    {
                        File.Delete(inputFile);
                    }
                    catch(Exception ex)
                    {
                        throw new FileLoadException(inputFile, ex);
                    }
                }
            }
            catch (FileLoadException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new MethodAccessException("UpdateMetaData", ex);
            }
        }

        private DocumentModel GetDocumentModel(Document document, string mediaFolderPath, string mediaBasePath, int lastPageNumberRead)
        {
            return document != null ? new DocumentModel
            (
                document,
                mediaFolderPath,
                mediaFolderPath,
                mediaBasePath,
                lastPageNumberRead
            ) : null;
        }

        public async Task<DocumentModel> GetAsync(int id, string mediaFolderPath, int userId)
        {
            var document = await biblioEntities.Documents.Include(x=> x.Category)
                .SingleOrDefaultAsync(x=> x.Id == id);
            if (document != null)
            {
                UserDocument userDocument = GetLastUserDocument(id, userId);

                var mediaBasePath = Path.Combine(env.WebRootPath, mediaFolderPath.Replace("~/", string.Empty));
                return GetDocumentModel(document, mediaFolderPath, mediaBasePath, userDocument?.LastPageNumber ?? 0);
            }
            return null;            
        }

        private UserDocument GetLastUserDocument(int documentId, int userId)
        {
            return biblioEntities.UserDocuments.OrderBy(x => x.ReadDate).LastOrDefault
            (
                x =>
                x.DocumentId == documentId &&
                x.UserId == userId
            );
        }

        public async Task<DocumentModel> GetAsync(string code, RoleOptions role, string mediaFolderPath, int userId)
        {
            var query = biblioEntities.Documents.Include(x => x.Category).Where(x => x.Code == code);
            if (role != RoleOptions.Librarian)
                query = query.Where(x => x.Status == (short)StatusOptions.Actived);

            var document = await query.SingleOrDefaultAsync();
            if (document != null)
            {
                var userDocument = GetLastUserDocument(document.Id, userId);

                var mediaBasePath = Path.Combine(env.WebRootPath, mediaFolderPath.Replace("~/", string.Empty));
                return GetDocumentModel(document, mediaFolderPath, mediaBasePath, userDocument?.LastPageNumber ?? 0);
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
                    OssFile.DeleteFile(document.Image, mediaBasePath);
                    OssFile.DeleteFile(document.File, mediaBasePath);
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

        public async Task IncrementCountReadAsync(string code, int userId)
        {
            try
            {
                //delete last year read
                biblioEntities.UserDocuments.RemoveRange
                (
                    biblioEntities.UserDocuments.Where
                    (
                        x => x.ReadDate <= DateTime.UtcNow.AddYears(-1)
                    )
                );
                code = code?.ToLower() ?? "";
                Document currenDocument = await biblioEntities.Documents.SingleOrDefaultAsync(x => x.Code.ToLower() == code);
                
                if (currenDocument != null)
                {
                    currenDocument.ReadCount++;
                    UserDocument currentUserDocument = GetLastUserDocument(currenDocument.Id, userId);
                    var newUserDocument = new UserDocument
                    (
                        0,
                        userId,
                        currenDocument.Id,
                        currentUserDocument?.LastPageNumber ?? 1,
                        DateTime.UtcNow
                    );
                    biblioEntities.UserDocuments.Add(newUserDocument);
                    await biblioEntities.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SaveLastReadAsync(string code, int userId, int? lastPageNumber)
        {
            try
            {
                code = code?.ToLower() ?? "";
                Document document = await biblioEntities.Documents.FirstOrDefaultAsync
                (
                    x =>
                    x.Code.ToLower() == code
                );
                if (document != null)
                {
                    UserDocument currentUserDocument = GetLastUserDocument(document.Id, userId);
                    var newUserDocument = new UserDocument
                    (
                        currentUserDocument?.Id ?? 0,
                        userId,
                        document.Id,
                        lastPageNumber ?? (currentUserDocument?.LastPageNumber ?? 1),
                        currentUserDocument?.ReadDate ?? DateTime.UtcNow
                    );
                    if (currentUserDocument == null)
                    {
                        biblioEntities.UserDocuments.Add(newUserDocument);
                    }
                    else
                    {
                        biblioEntities.Entry(currentUserDocument).CurrentValues.SetValues(newUserDocument);

                    }
                    await biblioEntities.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IDictionary<string, long>> ReadingCountPerMonth()
        {
            string[] month =
            {
                Text.Jan,
                Text.Feb,
                Text.Mar,
                Text.Apr,
                Text.May,
                Text.Jun,
                Text.Jul,
                Text.Aug,
                Text.Sep,
                Text.Oct,
                Text.Nov,
                Text.Dec
            };
            var data = new Dictionary<string, long>();
            for (int i = 0; i < DateTime.UtcNow.Month; i++)
            {
                data.Add(month[i], 0);
            }

            var query =  biblioEntities.UserDocuments.Where
            (
                x => x.ReadDate.Year == DateTime.UtcNow.Year
            ).
            GroupBy
            (
                x => new { x.ReadDate.Year, x.ReadDate.Month }
            )
            .Select
            (
                x => new { Month = x.Key.Month, Count = x.LongCount() }
            );

            var data2 = await query.ToDictionaryAsync
            (
                x => month[x.Month - 1],
                x => x.Count
            );

            foreach(var d in data2)
            {
                data[d.Key] = d.Value; 
            }            

            return data;
        }


        public async Task<IEnumerable<DocumentModel>> TopReading(int limit, string mediaFolderPath)
        {

            return
            (
                await biblioEntities.Documents.Where
                (
                    x => x.Status == (short)StatusOptions.Actived
                ).
                OrderByDescending
                (
                    x => x.ReadCount
                )
                .Take(limit).ToArrayAsync()
            )
            .Select
            (
                x => GetDocumentModel(x, mediaFolderPath, null, 0)
            );
        }

        public async Task<double> ReadCountAsync(IEnumerable<int> documentIds)
        {
            if ((documentIds?.Count() ?? 0) == 0)
                return 0;
            return await biblioEntities.Documents.Where
            (
                x =>
                documentIds.Contains(x.Id) &&
                x.Status == (short)StatusOptions.Actived
            ).SumAsync(x => x.ReadCount);
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

                if (OssFile.HasImage(documentModel.ImageUploaded))
                {
                    newImageName = OssFile.SaveImage(documentModel.ImageUploaded, 400, 600, 100 * 1024, 300 * 1024, prefixDocumentImageName, mediaAbsoluteBasePath); ;
                }
                else
                {
                    if (!documentModel.DeleteImage)
                    {
                        if (!string.IsNullOrEmpty(documentModel.ImageUploadedTmpFileName) &&
                        File.Exists(Path.Combine(mediaAbsoluteBaseTmpPath, documentModel.ImageUploadedTmpFileName)))
                        {
                            newImageName = OssFile.GetNewFileName(documentModel.ImageUploadedTmpFileName, prefixDocumentImageName);
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

                if (OssFile.HasFile(documentModel.FileUploaded))
                {
                    newFileName = await OssFile.SaveFile(documentModel.FileUploaded, prefixDocumentFileName, mediaAbsoluteBasePath); ;
                }
                else
                {
                    if (!string.IsNullOrEmpty(documentModel.FileUploadedTmpFileName) &&
                    File.Exists(Path.Combine(mediaAbsoluteBaseTmpPath, documentModel.FileUploadedTmpFileName)))
                    {
                        newFileName = OssFile.GetNewFileName(documentModel.FileUploadedTmpFileName, prefixDocumentFileName);
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
                return new DocumentModel(newDocument, mediaFolderPath, mediaFolderPath, null, 0);
            }
            catch (Exception ex)
            {
                if (OssFile.HasImage(documentModel.ImageUploaded) && !string.IsNullOrEmpty(newImageName))
                    OssFile.DeleteFile(newImageName, mediaAbsoluteBasePath);
                if (OssFile.HasFile(documentModel.FileUploaded) && !string.IsNullOrEmpty(newFileName))
                    OssFile.DeleteFile(newFileName, mediaAbsoluteBasePath);
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
                if (OssFile.HasImage(documentModel.ImageUploaded))
                {
                    newImageName = OssFile.SaveImage(documentModel.ImageUploaded, 400, 600, 100 * 1024, 300 * 1024, prefixDocumentImageName, mediaAbsoluteBasePath); ;
                    deleteCurrentIamge = true;
                }
                else if (string.IsNullOrEmpty(documentModel.ImageUploadedTmpFileName) && !string.IsNullOrEmpty(currentDocument.Image) && documentModel.DeleteImage)
                {
                    deleteCurrentIamge = true;
                }
                else if (!documentModel.DeleteImage && !string.IsNullOrEmpty(documentModel.ImageUploadedTmpFileName) &&
                    File.Exists(Path.Combine(mediaAbsoluteBaseTmpPath, documentModel.ImageUploadedTmpFileName)))
                {
                    newImageName = OssFile.GetNewFileName(documentModel.ImageUploadedTmpFileName, prefixDocumentImageName);
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


                if (OssFile.HasFile(documentModel.FileUploaded))
                {
                    newFileName = await OssFile.SaveFile(documentModel.FileUploaded, prefixDocumentFileName, mediaAbsoluteBasePath); ;
                    deleteCurrentFile = true;
                }
                else if (!string.IsNullOrEmpty(documentModel.FileUploadedTmpFileName) &&
                    File.Exists(Path.Combine(mediaAbsoluteBaseTmpPath, documentModel.FileUploadedTmpFileName)))
                {
                    newFileName = OssFile.GetNewFileName(documentModel.FileUploadedTmpFileName, prefixDocumentFileName);
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
                    OssFile.DeleteFile(currentDocumentImage, mediaAbsoluteBasePath);
                if(deleteCurrentFile)
                    OssFile.DeleteFile(currentDocumentFile, mediaAbsoluteBasePath);

                return new DocumentModel(newDocument, mediaFolderPath, mediaFolderPath, null, 0);
            }
            catch (Exception ex)
            {
                if(OssFile.HasImage(documentModel.ImageUploaded) && !string.IsNullOrEmpty(newImageName))
                    OssFile.DeleteFile(newImageName, mediaAbsoluteBasePath);
                if (OssFile.HasFile(documentModel.FileUploaded) && !string.IsNullOrEmpty(newFileName))
                    OssFile.DeleteFile(newFileName, mediaAbsoluteBasePath);
                throw ex;
            }
        }
    }
}
