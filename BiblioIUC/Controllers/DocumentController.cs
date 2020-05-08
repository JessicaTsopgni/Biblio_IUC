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
    public class DocumentController : BaseController
    {
        private readonly IDocumentLogic documentLogic;
        private readonly ICategoryLogic categoryLogic;
        private readonly IWebHostEnvironment env;

        public DocumentController(IDocumentLogic documentLogic, ICategoryLogic categoryLogic,
            IConfiguration configuration, ILoggerFactory loggerFactory, IWebHostEnvironment env) : base(configuration, loggerFactory)
        {
            this.documentLogic = documentLogic;
            this.categoryLogic = categoryLogic;
            this.env = env;
        }
        
        [AllowAnonymous]
        public string Test()
        {
            var mediaBasePath = Path.Combine(env.WebRootPath, configuration["MediaFolderPath"].Replace("~/", string.Empty));
            var libGostScriptPath = Path.Combine(env.WebRootPath, configuration["LibGostScriptPath"].Replace("~/", string.Empty));
            BiblioIUC.Logics.Tools.OssFile.ConvertPdfToImage
            (
                libGostScriptPath,
                Path.Combine(mediaBasePath, "cSharp.pdf"),
                1,
                Path.Combine(mediaBasePath, "thumb.png"),
                400,
                600
            );
            return Path.Combine(mediaBasePath, "thumb.png");
        }
        
        public async Task<IActionResult> Index(string categoryName, string documentIds, LayoutModel layoutModel)
        {
            try
            {
                ViewBag.SearchCategoryName = categoryName;
                var documentModels = await documentLogic.FindAsync
                (
                    documentIds: documentIds?.Split(',').Where(x=> int.TryParse(x, out _)).Select(x => int.Parse(x)).ToArray(),
                    value: layoutModel.SearchValue,
                    mediaFolderPath: configuration["MediaFolderPath"],
                    withDisabled: User.FindFirst(ClaimTypes.Role).Value == RoleOptions.Admin.ToString(),
                    orderBy: null,
                    orderByDescending: x => x.CreateDate,
                    pageIndex: layoutModel.PageIndex,
                    pageSize: layoutModel.PageSize
                );

                var pageListModel = new PageListModel<DocumentModel>
                (
                    documentModels,
                    layoutModel.ReturnUrl ?? currentUrl,
                    layoutModel.SearchValue,
                    layoutModel.PageIndex,
                    layoutModel.PageSize
                );

                if (IsAjax)
                    return PartialView("_IndexPartial", documentModels);
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

        public async Task<IActionResult> Read(string id, LayoutModel layoutModel)
        {
            try
            {
                var documentModel = await documentLogic.GetAsync
                (
                    id,
                    (RoleOptions)Enum.Parse(typeof(RoleOptions), User.FindFirst(ClaimTypes.Role).Value),
                    configuration["MediaFolderPath"]
                );
                if (documentModel == null)
                {
                    TempData["MessageType"] = MessageOptions.Warning;
                    TempData["Message"] = Text.Document_not_found;
                    if (!string.IsNullOrEmpty(layoutModel.ReturnUrl))
                        return Redirect(layoutModel.ReturnUrl);
                }
                else
                {
                    documentModel.SetCategoryModels
                    (
                        await categoryLogic.NoChildAsync(configuration["MediaFolderPath"]),
                        documentModel.CategoryId
                    );
                    var pageModel = new PageModel<DocumentModel>
                    (
                        documentModel,
                        layoutModel
                    );
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

        [AllowAnonymous]
        public async Task<IActionResult> GenerateCode()
        {
            try
            {
                return Json(await documentLogic.GenerateCodeAsync(configuration["DocumentCodePrefix"]));
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger(ex.GetType()).LogError($"{ex}\n\n");
                return Json(Text.An_error_occured);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(LayoutModel layoutModel)
        {
            try
            {
                PageModel<DocumentModel> pageModel = null;
                if (TempData["PageModel"] != null)
                {
                    pageModel = JsonConvert.DeserializeObject<PageModel<DocumentModel>>(TempData["PageModel"].ToString());
                }
                else
                {
                    DocumentModel documentModel = new DocumentModel
                    (
                        null,
                        await categoryLogic.NoChildAsync(configuration["MediaFolderPath"]),
                        null,
                        StatusOptions.Actived
                    );
                    pageModel = new PageModel<DocumentModel>
                    (
                        documentModel,
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id, LayoutModel layoutModel)
        {
            try
            {
                var documentModel = await documentLogic.GetAsync
                (
                    id ?? 0,
                    configuration["MediaFolderPath"]
                );
                if (documentModel == null)
                {
                    TempData["MessageType"] = MessageOptions.Warning;
                    TempData["Message"] = Text.Document_not_found;
                    if (!string.IsNullOrEmpty(layoutModel.ReturnUrl))
                        return Redirect(layoutModel.ReturnUrl);
                }
                else
                {
                    PageModel<DocumentModel> pageModel = null;
                    if (TempData["PageModel"] != null)
                    {
                        pageModel = JsonConvert.DeserializeObject<PageModel<DocumentModel>>(TempData["PageModel"].ToString());
                    }
                    else
                    {
                        documentModel.SetCategoryModels
                        (
                            await categoryLogic.NoChildAsync(configuration["MediaFolderPath"]),
                            documentModel.CategoryId
                        );
                        pageModel = new PageModel<DocumentModel>
                        (
                            documentModel,
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(PageModel<DocumentModel> pageModel)
        {
            try
            {
                pageModel.DataModel.SetCategoryModels
                (
                    await categoryLogic.NoChildAsync(configuration["MediaFolderPath"]),
                    pageModel.DataModel.CategoryId
                );
                if (pageModel.DataModel.ForExtraction)
                {
                    if (!Logics.Tools.OssFile.HasFile(pageModel.DataModel.FileUploaded))
                    {
                        TempData["MessageType"] = MessageOptions.Warning;
                        TempData["Message"] = string.Format(Text.The_fields_x_is_required, Text.The_document);
                        ModelState.AddModelError("DataModel.FileUploaded", TempData["Message"].ToString());
                        return View(pageModel);
                    }

                    int userId = 0;
                    if (!string.IsNullOrEmpty(User.Claims.FirstOrDefault(x => x.Type == "User.Id")?.Value))
                        userId = int.Parse(User.Claims.FirstOrDefault(x => x.Type == "User.Id")?.Value ?? "0");

                    pageModel.DataModel = await documentLogic.ExtractMetadata
                    (
                        pageModel.DataModel,
                        configuration["PrefixDocumentTemporyFileName"],
                        configuration["MediaFolderTemporyPath"],
                        configuration["LibGostScriptPath"],
                        userId
                    );
                    pageModel.DataModel.ForExtraction = false;
                    TempData["PageModel"] = JsonConvert.SerializeObject(pageModel);
                    if (pageModel.DataModel.Id == 0)
                        return RedirectToAction("Create", (LayoutModel)pageModel);
                    else
                        return RedirectToAction("Edit", new { id = pageModel.DataModel.Id, layoutModel = (LayoutModel)pageModel});
                }
                else
                {
                    if (ModelState.IsValid)
                    {
                        DocumentModel newDocumentModel = null;

                        TempData["MessageType"] = MessageOptions.Success;
                        TempData["Message"] = Text.Save_done;

                        if (pageModel.DataModel.Id == 0)
                        {
                            var mediaBaseTmpPath = Path.Combine(env.WebRootPath, configuration["MediaFolderTemporyPath"].Replace("~/", string.Empty).Replace("/", @"\"));

                            if ((string.IsNullOrEmpty(pageModel.DataModel.FileUploadedTmpFileName) || 
                                !System.IO.File.Exists(Path.Combine(mediaBaseTmpPath, pageModel.DataModel.FileUploadedTmpFileName))) 
                                && !Logics.Tools.OssFile.HasFile(pageModel.DataModel.FileUploaded))
                            {
                                TempData["MessageType"] = MessageOptions.Warning;
                                TempData["Message"] = string.Format(Text.The_fields_x_is_required, Text.The_document);
                                ModelState.AddModelError("DataModel.FileUploaded", TempData["Message"].ToString());
                                TempData["PageModel"] = JsonConvert.SerializeObject(pageModel);
                                return RedirectToAction("Edit", (LayoutModel)pageModel);
                            }
                            newDocumentModel = await documentLogic.AddAsync
                            (
                                pageModel.DataModel,
                                configuration["MediaFolderPath"],
                                configuration["MediaFolderTemporyPath"],
                                configuration["PrefixDocumentImageName"],
                                configuration["PrefixDocumentFileName"]
                            );
                            return RedirectToAction("Create", (LayoutModel)pageModel);
                        }
                        else
                        {
                            newDocumentModel = await documentLogic.SetAsync
                            (
                                pageModel.DataModel,
                                configuration["MediaFolderPath"],
                                configuration["MediaFolderTemporyPath"],
                                configuration["PrefixDocumentImageName"],
                                configuration["PrefixDocumentFileName"]
                            );
                        }

                        if (pageModel.DataModel.UpdateMetadata)
                        {
                            documentLogic.UpdateMetaData
                            (
                                configuration["MediaFolderPath"],
                                newDocumentModel
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
            }
            catch (KeyNotFoundException)
            {
                TempData["MessageType"] = MessageOptions.Warning;
                TempData["Message"] = Text.Data_not_exists_or_deleted;
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger(ex.GetType()).LogError($"{ex}\n\n");
                TempData["MessageType"] = MessageOptions.Danger;
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
                await documentLogic.RemoveAsync
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

        [AllowAnonymous]
        public async Task<JsonResult> CodeExists(PageModel<DocumentModel> pageModel)
        {
            bool b = await documentLogic.CodeAlreadyExistsAsync(pageModel.DataModel.Code, pageModel.DataModel.Id);
            return new JsonResult(!b);
        }
    }
}