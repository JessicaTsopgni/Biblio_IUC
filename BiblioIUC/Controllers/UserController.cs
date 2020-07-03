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
    [Authorize(Roles = "Admin, Librarian")]
    public class UserController : BaseController
    {
        private readonly IUserLogic userLogic;

        public UserController(IUserLogic userLogic, 
            IConfiguration configuration, ILoggerFactory loggerFactory):base(configuration, loggerFactory)
        {
            this.userLogic = userLogic;
        }

        public async Task<IActionResult> Index(int? id, int? UserParentId, LayoutModel layoutModel)
        {
            try
            {
                var userModels = await userLogic.FindAsync
                (                   
                    value: layoutModel.SearchValue,
                    mediaFolderPath: configuration["MediaFolderPath"],
                    withDisabled: User.FindFirst(ClaimTypes.Role).Value == RoleOptions.Librarian.ToString(),
                    orderBy: x => x.FullName,
                    pageIndex: layoutModel.PageIndex,
                    pageSize: layoutModel.PageSize
                );

                var pageListModel = new PageListModel<UserModel>
                (
                    userModels,
                    layoutModel.ReturnUrl ?? currentUrl,
                    layoutModel.SearchValue,
                    layoutModel.PageIndex,
                    layoutModel.PageSize,
                    Text.Searh_a_user,
                    "Index",
                    "User"
                );

                if (IsAjax)
                    return PartialView("_IndexPartial", userModels);
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

        public IActionResult Create(LayoutModel layoutModel)
        {
            try
            {
                UserModel userModel = new UserModel
                (
                    new User
                    {
                        Status = (short)StatusOptions.Actived,
                        Role = (short)RoleOptions.Student
                    },
                    configuration["MediaFolderPath"]
                );
                var pageModel = new PageModel<UserModel>
                (
                    userModel,
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

        public async Task<IActionResult> Edit(int? id, LayoutModel layoutModel)
        {
            try
            {
                var userModel = await userLogic.GetAsync
                (
                    id ?? 0,
                    configuration["MediaFolderPath"]
                );
                if (userModel == null)
                {
                    TempData["MessageType"] = MessageOptions.Warning;
                    TempData["Message"] = Text.User_not_found;
                    if (!string.IsNullOrEmpty(layoutModel.ReturnUrl))
                        return Redirect(layoutModel.ReturnUrl);
                }
                else
                {                    
                    var pageModel = new PageModel<UserModel>
                    (
                        userModel,
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
        public async Task<IActionResult> Edit(PageModel<UserModel> pageModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    UserModel newUserModel = null;

                    TempData["MessageType"] = MessageOptions.Success;
                    TempData["Message"] = Text.Save_done;

                    if (pageModel.DataModel.Id == 0)
                    {
                        newUserModel = await userLogic.AddAsync
                        (
                            pageModel.DataModel,
                            configuration["MediaFolderPath"],
                            configuration["PrefixUserImageName"]
                        );
                        return RedirectToAction("Index", new { pageModel.ReturnUrl });
                    }
                    else
                    {
                        var userModel = await userLogic.GetAsync
                        (
                            pageModel.DataModel.Id,
                            configuration["MediaFolderPath"]
                        );
                        if (userModel == null)
                        {
                            TempData["MessageType"] = MessageOptions.Warning;
                            TempData["Message"] = Text.User_not_found;
                            return RedirectToAction("Index", new { pageModel.ReturnUrl });
                        }
                        pageModel.DataModel.Copy(userModel);
                        newUserModel = await userLogic.SetAsync
                        (
                            pageModel.DataModel,
                            configuration["MediaFolderPath"],
                            configuration["PrefixUserImageName"],
                            false
                        );
                    }
                }
                else
                {
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


        public async Task<IActionResult> Delete(int? id, string returnUrl = null)
        {
            try
            {
                await userLogic.RemoveAsync
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
        public async Task<JsonResult> AccountExists(PageModel<UserModel> pageModel)
        {
            bool b = await userLogic.AccountAlreadyExistsAsync(pageModel.DataModel.Account, pageModel.DataModel.Id);
            return new JsonResult(!b);
        }
    }
}