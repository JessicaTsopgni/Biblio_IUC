using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BiblioIUC.Entities;
using BiblioIUC.Localize;
using BiblioIUC.Logics;
using BiblioIUC.Logics.Interfaces;
using BiblioIUC.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BiblioIUC.Controllers
{
    [Authorize]
    public class CategoryController : BaseController
    {
        private readonly ICategoryLogic categoryLogic;

        public CategoryController(ICategoryLogic categoryLogic, 
            IConfiguration configuration, ILoggerFactory loggerFactory):base(configuration, loggerFactory)
        {
            this.categoryLogic = categoryLogic;
        }

        public async Task<IActionResult> Index(int? id, int? CategoryParentId, LayoutModel layoutModel)
        {
            try
            {
                var categoryModels = await categoryLogic.FindAsync
                (
                    id: id ?? 0,
                    categoryParentId: CategoryParentId ?? 0,
                    value: layoutModel.SearchValue,
                    mediaFolderPath: configuration["MediaFolderPath"],
                    withDisabled: User.FindFirst(ClaimTypes.Role).Value == RoleOptions.Admin.ToString(),
                    orderBy: x => x.Name,
                    pageIndex: layoutModel.PageIndex,
                    pageSize: layoutModel.PageSize
                );

                var pageListModel = new PageListModel<CategoryModel>
                (
                    categoryModels,
                    layoutModel.ReturnUrl ?? currentUrl,
                    layoutModel.SearchValue,
                    layoutModel.PageIndex,
                    layoutModel.PageSize,
                    Text.Searh_a_category,
                    "Index",
                    "Category"
                );

                if (IsAjax)
                    return PartialView("_IndexPartial", categoryModels);
                return View(pageListModel);
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger(ex.GetType()).LogError($"{ex}\n\n");
                TempData["MessageType"] = MessageOptions.Warning;
                TempData["Message"] = Text.An_error_occured;
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(LayoutModel layoutModel)
        {
            try
            {
                CategoryModel categoryModel = new CategoryModel
                (
                    await categoryLogic.NoDocumentAsync(configuration["MediaFolderPath"]),
                    null,
                    StatusOptions.Actived
                );
                var pageModel = new PageModel<CategoryModel>
                (
                    categoryModel,
                    layoutModel
                );
                return View("Edit", pageModel);
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger(ex.GetType()).LogError($"{ex}\n\n");
                TempData["MessageType"] = MessageOptions.Warning;
                TempData["Message"] = Text.An_error_occured;
            }
            if (!string.IsNullOrEmpty(layoutModel.ReturnUrl))
                return Redirect(layoutModel.ReturnUrl);
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id, LayoutModel layoutModel)
        {
            try
            {
                var categoryModel = await categoryLogic.GetAsync
                (
                    id ?? 0,
                    configuration["MediaFolderPath"]
                );
                if (categoryModel == null)
                {
                    TempData["MessageType"] = MessageOptions.Warning;
                    TempData["Message"] = Text.Category_not_found;
                    if (!string.IsNullOrEmpty(layoutModel.ReturnUrl))
                        return Redirect(layoutModel.ReturnUrl);
                }
                else
                {
                    categoryModel.SetCategoryParentModels
                    (
                        await categoryLogic.NoDocumentAsync(configuration["MediaFolderPath"]),
                        categoryModel.CategoryParentId
                    );
                    var pageModel = new PageModel<CategoryModel>
                    (
                        categoryModel,
                        layoutModel
                    );
                    return View(pageModel);
                }
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger(ex.GetType()).LogError($"{ex}\n\n");
                TempData["MessageType"] = MessageOptions.Warning;
                TempData["Message"] = Text.An_error_occured;
            }
            return RedirectToAction("Index");
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(PageModel<CategoryModel> pageModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    CategoryModel newCategoryModel = null;

                    TempData["MessageType"] = MessageOptions.Success;
                    TempData["Message"] = Text.Save_done;

                    if (pageModel.DataModel.Id == 0)
                    {
                        newCategoryModel = await categoryLogic.AddAsync
                        (
                            pageModel.DataModel,
                            configuration["MediaFolderPath"],
                            configuration["PrefixCategoryImageName"]
                        );
                        return RedirectToAction("Create", new { pageModel.ReturnUrl });
                    }
                    else
                    {
                        newCategoryModel = await categoryLogic.SetAsync
                        (
                            pageModel.DataModel,
                            configuration["MediaFolderPath"],
                            configuration["PrefixCategoryImageName"]
                        );
                    }
                }
                else
                {
                    pageModel.DataModel.SetCategoryParentModels
                    (
                        await categoryLogic.NoDocumentAsync(configuration["MediaFolderPath"]),
                        pageModel.DataModel.CategoryParentId
                    );
                    TempData["MessageType"] = MessageOptions.Warning;
                    TempData["Message"] = "<ul>";
                    foreach (var v in ModelState.Values)
                        foreach (var vv in v.Errors)
                            TempData["Message"] += $"<li>{(!string.IsNullOrEmpty(vv.ErrorMessage) ? vv.ErrorMessage : vv.Exception.Message)}</li>";
                    TempData["Message"] += "</ul>";
                    return View(pageModel);
                }
            }
            catch (KeyNotFoundException)
            {
                TempData["MessageType"] = MessageOptions.Warning;
                TempData["Message"] = Text.Data_not_exists_or_deleted;
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger(ex.GetType()).LogError($"{ex}\n\n");
                TempData["MessageType"] = MessageOptions.Warning;
                TempData["Message"] = Text.An_error_occured;
            }
            if (!string.IsNullOrEmpty(pageModel.ReturnUrl))
                return Redirect(pageModel.ReturnUrl);
            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id, string returnUrl = null)
        {
            try
            {
                await categoryLogic.RemoveAsync
                (
                    id ?? 0,
                    configuration["MediaFolderPath"]
                );

                TempData["MessageType"] = MessageOptions.Success;
                TempData["Message"] = Text.Delete_done;
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger(ex.GetType()).LogError($"{ex}\n\n");
                TempData["MessageType"] = MessageOptions.Warning;
                TempData["Message"] = Text.An_error_occured;
            }
            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public async Task<JsonResult> NameExists(PageModel<CategoryModel> pageModel)
        {
            bool b = await categoryLogic.NameAlreadyExistsAsync(pageModel.DataModel.Name, pageModel.DataModel.Id);
            return new JsonResult(!b);
        }
    }
}