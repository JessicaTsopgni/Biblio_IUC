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

    public class CategoryLogic : ICategoryLogic
    {
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_PAGE_SIZE = 10;

        private readonly BiblioEntities biblioEntities;
        private readonly IWebHostEnvironment env;

        public CategoryLogic(BiblioEntities biblioEntities, IWebHostEnvironment env)
        {
            this.biblioEntities = biblioEntities;
            this.env = env;
        }

        public async Task<IEnumerable<CategoryModel>> FindAsync(int id, int categoryParentId, string value, string mediaFolderPath,
            Expression<Func<Category, object>> orderBy,
            bool withDisabled = false,  int pageIndex = DEFAULT_PAGE_INDEX, 
            int pageSize = DEFAULT_PAGE_SIZE)
        {
            value = value?.ToLower() ?? string.Empty;
            pageIndex = pageIndex < DEFAULT_PAGE_INDEX ? DEFAULT_PAGE_INDEX : pageIndex;
            pageSize = pageSize < pageIndex ? DEFAULT_PAGE_SIZE : pageSize;
            var query = biblioEntities.Categories
            .Include(x => x.CategoryParent)
            .Include(x => x.InverseCategoryParent)
            .Include(x => x.Documents)
            .Where(x => true);
            if (id > 0)
            {
                query = query.Where(x => x.CategoryParentId == id);
            }
            else if (categoryParentId > 0)
            {
                var categoryOfParentParentId = (await biblioEntities.Categories.FindAsync(categoryParentId))?.CategoryParentId;
                 query = query.Where(x => x.CategoryParentId == categoryOfParentParentId);
            }
            else if(string.IsNullOrWhiteSpace(value))
            {
                query = query.Where
                (
                    x => x.CategoryParentId == null
                );
            }
            else
            {
                query = query.Where
                (
                    x =>
                    (x.Name.ToLower().Contains(value) || value.Contains(x.Name)) ||
                    (x.Description != null && (x.Description.ToLower().Contains(value) || value.Contains(x.Description)))
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
                x => GetCategoryModel(x, mediaFolderPath)
            ).ToArray();
        }

        private CategoryModel GetCategoryModel(Category category, string mediaFolderPath)
        {
            return category != null ? new CategoryModel
            (
                category,
                mediaFolderPath
            ) : null;
        }

        public async Task<IEnumerable<CategoryModel>> LeafsAsync(string mediaFolderPath)
        {
            return
            (
                await biblioEntities.Categories.Where
                (
                    x =>
                    x.Documents.Count() == 0
                ).OrderBy(x => x.Name).ToArrayAsync()
            )
            .Select
            (
                x => GetCategoryModel(x, mediaFolderPath)
            );            
        }

        public async Task<IEnumerable<CategoryModel>> NoChildAsync(string mediaFolderPath)
        {
            return
            (
                await biblioEntities.Categories.Where
                (
                    x =>
                    x.InverseCategoryParent.Count() == 0
                ).OrderBy(x => x.Name).ToArrayAsync()
            )
            .Select
            (
                x => GetCategoryModel(x, mediaFolderPath)
            );
        }

        public async Task<CategoryModel> GetAsync(int id, string mediaFolderPath)
        {
            var category = await biblioEntities.Categories.FindAsync(id);
            if (category != null)
                return GetCategoryModel(category, mediaFolderPath);
            return null;            
        }


        public async Task RemoveAsync(int id, string mediaFolderPath)
        {
            var category = await biblioEntities.Categories.FindAsync(id);
            if (category != null)
            {
                biblioEntities.Categories.Remove(category);
                await biblioEntities.SaveChangesAsync();
                if(!string.IsNullOrEmpty(category.Image))
                {
                    var mediaBasePath = Path.Combine(env.WebRootPath, mediaFolderPath.Replace("~/", string.Empty));
                    Tools.OssFile.DeleteFile(category.Image, mediaBasePath);
                }
            }
        }

        public async Task<bool> NameAlreadyExistsAsync(string name, int id)
        {
            try
            {
                return await biblioEntities.Categories.SingleOrDefaultAsync
                (
                    x =>
                    x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                    x.Id != id
                ) != null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<CategoryModel> AddAsync(CategoryModel categoryModel,
            string mediaFolderPath, string PrefixCategoryImageName)
        {
            var mediaBasePath = Path.Combine(env.WebRootPath, mediaFolderPath.Replace("~/", string.Empty));
            string newFileName = null;
            try
            {
                if (categoryModel == null)
                    throw new ArgumentNullException("categoryModel");


                if (Tools.OssFile.HasImage(categoryModel.ImageUploaded))
                {
                    newFileName = Tools.OssFile.SaveImage(categoryModel.ImageUploaded, 300, 300, 100 * 1024, 200 * 1024, PrefixCategoryImageName, mediaBasePath); ;
                }

                Category newCategory = new Category
                (
                    categoryModel.Id,
                    categoryModel.Name,
                    categoryModel.Description,
                    categoryModel.CategoryParentId,
                    null,
                    newFileName,
                    (short)categoryModel.Status
                );

                biblioEntities.Categories.Add(newCategory);
                await biblioEntities.SaveChangesAsync();
                return new CategoryModel(newCategory, mediaFolderPath);
            }
            catch (Exception ex)
            {
                if (Tools.OssFile.HasImage(categoryModel.ImageUploaded) && !string.IsNullOrEmpty(newFileName))
                    Tools.OssFile.DeleteFile(newFileName, mediaBasePath);
                throw ex;
            }
        }

        public async Task<CategoryModel> SetAsync(CategoryModel categoryModel, 
            string mediaFolderPath, string PrefixCategoryImageName)
        {
            string newFileName = null;
            var mediaBasePath = Path.Combine(env.WebRootPath, mediaFolderPath?.Replace("~/", string.Empty));
            try
            {
                if (categoryModel == null)
                    throw new ArgumentNullException("categoryModel");

                var currentCategory = await biblioEntities.Categories.FindAsync(categoryModel.Id);
                if (currentCategory == null)
                    throw new KeyNotFoundException("Category");


                if (Tools.OssFile.HasImage(categoryModel.ImageUploaded))
                {
                    newFileName = Tools.OssFile.SaveImage(categoryModel.ImageUploaded, 300, 300, 100 * 1024, 200 * 1024, PrefixCategoryImageName, mediaBasePath); ;
                    Tools.OssFile.DeleteFile(currentCategory.Image, mediaBasePath);
                }
                else if (!string.IsNullOrEmpty(currentCategory.Image) && categoryModel.DeleteImage)
                {
                    Tools.OssFile.DeleteFile(currentCategory.Image, mediaBasePath);
                }
                else
                {
                    newFileName = currentCategory.Image;
                }
                Category newCategory = new Category
                (
                    categoryModel.Id,
                    categoryModel.Name,
                    categoryModel.Description,
                    categoryModel.CategoryParentId,
                    null,
                    newFileName,
                    (short)categoryModel.Status
                );

                biblioEntities.Entry(currentCategory).CurrentValues.SetValues(newCategory);
                await biblioEntities.SaveChangesAsync();
                return new CategoryModel(newCategory, mediaFolderPath);
            }
            catch (Exception ex)
            {
                if(Tools.OssFile.HasImage(categoryModel.ImageUploaded) && !string.IsNullOrEmpty(newFileName))
                    Tools.OssFile.DeleteFile(newFileName, mediaBasePath);
                throw ex;
            }
        }
    }
}
