using BiblioIUC.Entities;
using BiblioIUC.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BiblioIUC.Logics.Interfaces
{
    public interface ICategoryLogic
    {
        Task<IEnumerable<CategoryModel>> NoDocumentAsync(string mediaFolderPath);
        Task<IEnumerable<CategoryModel>> FindAsync(int id, int categoryParentId, string value, string mediaFolderPath,
            Expression<Func<Category, object>> orderBy, bool withDisabled = false, int pageIndex = 1, int pageSize = 10);
        Task<CategoryModel> GetAsync(int id, string mediaFolderPath);
        Task<CategoryModel> SetAsync(CategoryModel categoryModel, 
            string mediaFolderPath, string PrefixCategoryImageName);
        Task<bool> NameAlreadyExistsAsync(string name, int id);
        Task<CategoryModel> AddAsync(CategoryModel categoryModel, string mediaFolderPath, string PrefixCategoryImageName);
        Task RemoveAsync(int id, string mediaFolderPath);
        Task<IEnumerable<CategoryModel>> NoChildAsync(string mediaFolderPath);
        List<int> DocumentIds(int categoryId);
        CategoryModel Get(int id, string mediaFolderPath);
        Task<IEnumerable<CategoryModel>> GetRootsAsync(string mediaFolderPath);
        Task<IDictionary<CategoryModel, double>> TopSubCategoriesReading(int limit, string mediaFolderPath);
    }
}