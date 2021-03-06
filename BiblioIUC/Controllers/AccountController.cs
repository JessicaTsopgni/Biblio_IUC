﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BiblioIUC.Entities;
using BiblioIUC.Localize;
using BiblioIUC.Logics;
using BiblioIUC.Logics.Interfaces;
using BiblioIUC.Logics.Tools;
using BiblioIUC.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;

namespace BiblioIUC.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private readonly IUserLogic userLogic;
        private readonly ISuggestionLogic suggestionLogic;
        private readonly ILDAPAuthenticationService authService;

        public AccountController(IUserLogic userLogic, ISuggestionLogic suggestionLogic,
            IConfiguration configuration, ILoggerFactory loggerFactory,
            ILDAPAuthenticationService lDAPAuthenticationService) :base(configuration, loggerFactory)
        {
            this.userLogic = userLogic;
            this.suggestionLogic = suggestionLogic;
            authService = lDAPAuthenticationService;
        }

        public string Test(string id)
        {
            return Logics.Tools.MD5.Hash(id);
        }

        public IActionResult Index()
        {
            return RedirectToAction("Login");
            //return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Login(LayoutModel layoutModel)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            var loginModel = new LoginModel
            (
                "jessica",
                "Jessica@2020"
            );
            await layoutModel.Refresh(suggestionLogic, int.Parse(configuration["DashboardTopSuggestionLimit"]), configuration["MediaFolderPath"]);
            var pageModel = new PageModel<LoginModel>
            (
                loginModel,
                layoutModel
            );
            return View(pageModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(PageModel<LoginModel> pageModel)
        {
            try
            {
                //log in with ldap
                try
                {
                    var ldapUser = authService.Login(pageModel.DataModel.Account, pageModel.DataModel.Password);
                    if(ldapUser == null)
                        throw new MemberAccessException(Text.Account_or_password_is_incorrect);

                    UserModel userModel = new UserModel(ldapUser);
                    
                    var user = await userLogic.GetAsync(pageModel.DataModel.Account, configuration["MediaFolderPath"]);
                    
                    if(user == null)
                    {
                        userModel = await userLogic.AddAsync
                        (
                            userModel,
                            configuration["MediaFolderPath"],
                            configuration["PrefixPhotoProfileName"]
                        );
                    }
                    else
                    {
                        if(user.Status != StatusOptions.Actived)
                        {
                            throw new UnauthorizedAccessException(Text.This_account_has_been_disabled);
                        }
                        //le role et le status reste ce qui a été défini dans biblio
                        userModel.Id = user.Id;
                        userModel.Status = user.Status;
                        userModel.Role = user.Role;
                        userModel = await userLogic.SetAsync
                       (
                           userModel,
                           configuration["MediaFolderPath"],
                           configuration["PrefixPhotoProfileName"],
                           true
                       );
                    }

                    ProfileModel profileModel = new ProfileModel
                    (
                        userModel
                    );

                    userLogic.SignIn
                    (
                        profileModel,
                        Request.HttpContext
                    );
                }
                catch (LdapException ex)
                {
                    if(ex.ResultCode == 49)
                        throw new MemberAccessException(Text.Account_or_password_is_incorrect);
                    throw ex;
                }
                catch (UnauthorizedAccessException ex)
                {
                     throw ex;
                }
                catch (Exception ex)
                {
                    loggerFactory.CreateLogger(ex.GetType()).LogError($"{ex}\n\n");
                    var profileModel = await userLogic.LoginAsync
                    (
                        pageModel.DataModel,
                        Request.HttpContext,
                        configuration["MediaFolderPath"]
                    );
                }
                if (!string.IsNullOrEmpty(pageModel.ReturnUrl))
                    return Redirect(pageModel.ReturnUrl);
                return RedirectToAction("Index", "Home");
            }
            catch (MemberAccessException ex)
            {
                TempData["MessageType"] = MessageOptions.Warning;
                TempData["Message"] = ex.Message;
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["MessageType"] = MessageOptions.Warning;
                TempData["Message"] = ex.Message;
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger(ex.GetType()).LogError($"{ex}\n\n");
                TempData["MessageType"] = MessageOptions.Warning;
                TempData["Message"] = Text.An_error_occured;
            }
            return View(pageModel);
        }

        public IActionResult Logout()
        {
            try
            {
                userLogic.SignOut(HttpContext);
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger(ex.GetType()).LogError($"{ex}\n\n");
                TempData["MessageType"] = MessageOptions.Warning;
                TempData["Message"] = Text.An_error_occured;
            }
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Profile(LayoutModel layoutModel)
        {
            try
            {                
                var userModel = await userLogic.GetAsync
                (
                    HttpContext,
                    configuration["MediaFolderPath"]
                );
                var profileModel = new ProfileModel(userModel);
                await layoutModel.Refresh(suggestionLogic, int.Parse(configuration["DashboardTopSuggestionLimit"]), configuration["MediaFolderPath"]);
                var pageModel = new PageModel<ProfileModel>
                (
                    profileModel,
                    layoutModel
                );
                ViewBag.Account = pageModel.DataModel.Account;
                return View(pageModel);
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger(ex.GetType()).LogError($"{ex}\n\n");
                TempData["MessageType"] = MessageOptions.Warning;
                TempData["Message"] = Text.An_error_occured;
            }
            return RedirectToAction("Logout");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(PageModel<ProfileModel> pageModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var newProfileModel = await userLogic.EditProfilAsync
                    (
                        pageModel.DataModel,
                        Request,
                        configuration["MediaFolderPath"],
                        configuration["PrefixPhotoProfileName"]
                    );

                    userLogic.SignOut(Request.HttpContext);
                    userLogic.SignIn(newProfileModel, Request.HttpContext);

                    TempData["MessageType"] = MessageOptions.Success;
                    TempData["Message"] = Text.Save_done;
                    if(!string.IsNullOrEmpty(pageModel.ReturnUrl))
                        return Redirect(pageModel.ReturnUrl);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["MessageType"] = MessageOptions.Warning;
                    TempData["Message"] = "<ul>";
                    foreach (var v in ModelState.Values)
                        foreach (var vv in v.Errors)
                            TempData["Message"] += $"<li>{(!string.IsNullOrEmpty(vv.ErrorMessage) ? vv.ErrorMessage : vv.Exception.Message)}</li>";
                    TempData["Message"] += "</ul>";
                }
            }
            catch(KeyNotFoundException)
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
            return View(pageModel);
        }

        [AllowAnonymous]
        public async Task<JsonResult> AccountExists(string name, int id)
        {
            bool b = await userLogic.AccountAlreadyExistsAsync(name, id);
            return new JsonResult(!b);
        }

    }
}