using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ghostscript.NET;
using Newtonsoft.Json;

namespace BiblioIUC.Controllers
{
    [Authorize]
    public class SuggestionController : BaseController
    {
        private readonly ISuggestionLogic suggestionLogic;
        private readonly IUserLogic userLogic;
        private readonly IWebHostEnvironment env;

        public SuggestionController(ISuggestionLogic suggestionLogic, IUserLogic userLogic,
            IConfiguration configuration, ILoggerFactory loggerFactory, IWebHostEnvironment env) : base(configuration, loggerFactory)
        {
            this.suggestionLogic = suggestionLogic;
            this.userLogic = userLogic;
            this.env = env;
        }
        
       
        
        public async Task<IActionResult> Index(LayoutModel layoutModel)
        {
            try
            {
                var suggestionModels = await suggestionLogic.FindAsync
                (
                    value: layoutModel.SearchValue,
                    mediaFolderPath: configuration["MediaFolderPath"],
                    userId: (User.FindFirst(ClaimTypes.Role).Value != RoleOptions.Librarian.ToString() &&
                    User.FindFirst(ClaimTypes.Role).Value != RoleOptions.Admin.ToString()) ? (int?)int.Parse(User.Claims.FirstOrDefault(x => x.Type == "User.Id")?.Value ?? "0") : null,
                    orderBy: null,
                    orderByDescending: x => x.Date,
                    pageIndex: layoutModel.PageIndex,
                    pageSize: layoutModel.PageSize
                );

                var pageListModel = new PageListModel<SuggestionModel>
                (
                    suggestionModels,
                    layoutModel.ReturnUrl ?? currentUrl,
                    layoutModel.SearchValue,
                    layoutModel.PageIndex,
                    layoutModel.PageSize,
                    await suggestionLogic.UnReadCountAsync(),
                    await suggestionLogic.TopSuggestions(int.Parse(configuration["DashboardTopSuggestionLimit"]), configuration["MediaFolderPath"]),
                    Text.Searh_a_suggestion,
                    "Index",
                    "Suggestion"
                );

                if (IsAjax)
                    return PartialView("_IndexPartial", suggestionModels);
                return View(pageListModel);
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger(ex.GetType()).LogError($"{ex}\n\n");
                TempData["MessageType"] = MessageOptions.Danger;
                TempData["Message"] = Text.An_error_occured;
            }
            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> Create(LayoutModel layoutModel)
        {
            try
            {
                PageModel<SuggestionModel> pageModel = null;
                if (TempData["PageModel"] != null)
                {
                    pageModel = JsonConvert.DeserializeObject<PageModel<SuggestionModel>>(TempData["PageModel"].ToString());
                }
                else
                {
                    int userId = 0;
                    if (!string.IsNullOrEmpty(User.Claims.FirstOrDefault(x => x.Type == "User.Id")?.Value))
                        userId = int.Parse(User.Claims.FirstOrDefault(x => x.Type == "User.Id")?.Value ?? "0");

                    var userModel = await userLogic.GetAsync(userId, configuration["MediaFolderPath"]);

                    SuggestionModel suggestionModel = new SuggestionModel
                    (
                        new Suggestion
                        (
                            0,
                            null,
                            null,
                            null,
                            false,
                            false,
                            DateTime.UtcNow,
                            userId,
                            new User
                            (
                                userModel.Id,
                                userModel.Account,
                                userModel.Password,
                                userModel.FullName,
                                (short)userModel.Role,
                                userModel.ImageName,
                                (short)userModel.Status
                            )

                        ),
                        configuration["MediaFolderPath"],
                        configuration["MediaFolderPath"]
                    );
                    await layoutModel.Refresh(suggestionLogic, int.Parse(configuration["DashboardTopSuggestionLimit"]), configuration["MediaFolderPath"]);
                    pageModel = new PageModel<SuggestionModel>
                    (
                        suggestionModel,
                        layoutModel
                    );
                }
                return View("Edit", pageModel);
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger(ex.GetType()).LogError($"{ex}\n\n");
                TempData["MessageType"] = MessageOptions.Danger;
                TempData["Message"] = Text.An_error_occured;
            }
            if (!string.IsNullOrEmpty(layoutModel.ReturnUrl))
                return Redirect(layoutModel.ReturnUrl);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int? id, LayoutModel layoutModel)
        {
            try
            {

                var suggestionModel = await suggestionLogic.GetAndSetAsReadAsync
                (
                    id ?? 0,
                    configuration["MediaFolderPath"]
                );
                if (suggestionModel == null)
                {
                    TempData["MessageType"] = MessageOptions.Warning;
                    TempData["Message"] = Text.Suggestion_not_found;
                    if (!string.IsNullOrEmpty(layoutModel.ReturnUrl))
                        return Redirect(layoutModel.ReturnUrl);
                }
                else
                {
                    PageModel<SuggestionModel> pageModel = null;
                    if (TempData["PageModel"] != null)
                    {
                        pageModel = JsonConvert.DeserializeObject<PageModel<SuggestionModel>>(TempData["PageModel"].ToString());
                    }
                    else
                    {
                        await layoutModel.Refresh(suggestionLogic, int.Parse(configuration["DashboardTopSuggestionLimit"]), configuration["MediaFolderPath"]);
                        pageModel = new PageModel<SuggestionModel>
                        (
                            suggestionModel,
                            layoutModel
                        );
                    }
                    return View(pageModel);
                }
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger(ex.GetType()).LogError($"{ex}\n\n");
                TempData["MessageType"] = MessageOptions.Danger;
                TempData["Message"] = Text.An_error_occured;
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int? id, LayoutModel layoutModel)
        {
            try
            {
               

                var suggestionModel = await suggestionLogic.GetAsync
                (
                    id ?? 0,
                    configuration["MediaFolderPath"]
                );
                if (suggestionModel == null)
                {
                    TempData["MessageType"] = MessageOptions.Warning;
                    TempData["Message"] = Text.Suggestion_not_found;
                    if (!string.IsNullOrEmpty(layoutModel.ReturnUrl))
                        return Redirect(layoutModel.ReturnUrl);
                }
                else
                {
                    PageModel<SuggestionModel> pageModel = null;
                    if (TempData["PageModel"] != null)
                    {
                        pageModel = JsonConvert.DeserializeObject<PageModel<SuggestionModel>>(TempData["PageModel"].ToString());
                    }
                    else
                    {
                        await layoutModel.Refresh(suggestionLogic, int.Parse(configuration["DashboardTopSuggestionLimit"]), configuration["MediaFolderPath"]);
                        pageModel = new PageModel<SuggestionModel>
                        (
                            suggestionModel,
                            layoutModel
                        );
                    }
                    return View(pageModel);
                }
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger(ex.GetType()).LogError($"{ex}\n\n");
                TempData["MessageType"] = MessageOptions.Danger;
                TempData["Message"] = Text.An_error_occured;
            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824)]
        [RequestSizeLimit(1073741824)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PageModel<SuggestionModel> pageModel)
        {
            try
            {
                
                    if (ModelState.IsValid)
                    {
                        SuggestionModel newSuggestionModel = null;

                        TempData["MessageType"] = MessageOptions.Success;
                        TempData["Message"] = Text.Save_done;

                        if (pageModel.DataModel.Id == 0)
                        {
                            
                            newSuggestionModel = await suggestionLogic.AddAsync
                            (
                                pageModel.DataModel,
                                configuration["MediaFolderPath"],
                                configuration["PrefixSuggestionFileName"]
                            );
                        }
                        else
                        {
                            newSuggestionModel = await suggestionLogic.SetAsync
                            (
                                pageModel.DataModel,
                                configuration["MediaFolderPath"],
                                configuration["MediaFolderTemporyPath"],
                                configuration["PrefixSuggestionImageName"],
                                configuration["PrefixSuggestionFileName"]
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
            catch (MethodAccessException ex)
            {
                loggerFactory.CreateLogger(ex.GetType()).LogError($"{ex}\n\n");
                TempData["MessageType"] = MessageOptions.Warning;
                TempData["Message"] = Text.Save_done_but_without_update_metadata;
            }
            catch (FileLoadException ex)
            {
                loggerFactory.CreateLogger(ex.GetType()).LogError($"{ex}\n\n");
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger(ex.GetType()).LogError($"{ex}\n\n");
                TempData["MessageType"] = MessageOptions.Danger;
                TempData["Message"] = Text.An_error_occured;
            }
            //if(pageModel.DataModel.Id == 0)
            //    return RedirectToAction("Create", (LayoutModel)pageModel);

            if (!string.IsNullOrEmpty(pageModel.ReturnUrl))
                return Redirect(pageModel.ReturnUrl);
            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Admin, Librarian, Teacher")]
        public async Task<IActionResult> Delete(int? id, string returnUrl = null)
        {
            try
            {
                await suggestionLogic.RemoveAsync
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
                TempData["MessageType"] = MessageOptions.Danger;
                TempData["Message"] = Text.An_error_occured;
            }
            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index");
        }

    }
}