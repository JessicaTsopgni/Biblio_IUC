using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using BiblioIUC.Entities;
using BiblioIUC.Logics;
using BiblioIUC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BiblioIUC.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IConfiguration configuration;
        protected readonly ILoggerFactory loggerFactory;
        protected bool IsAjax = false;
        protected string currentUrl;
        public BaseController(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            this.configuration = configuration;
            this.loggerFactory = loggerFactory;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.AppName = configuration["AppName"];
            ViewBag.Url = RemoveQueryStringByKey(Request.GetDisplayUrl(), "ReturnUrl");
            ViewBag.InfiniteScrollUrl = RemoveQueryStringByKey(ViewBag.Url, "PageIndex");
            ViewBag.InfiniteScrollUrl = RemoveQueryStringByKey(ViewBag.InfiniteScrollUrl, "PageSise");
            IsAjax = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (string.IsNullOrEmpty(context.HttpContext.Request.Headers["Referer"].ToString()) ||
                context.HttpContext.Request.Headers["Referer"].ToString() == Request.GetDisplayUrl())
                ViewBag.RefererUrl = Url.Action("Index","Home");
            else
                ViewBag.RefererUrl = context.HttpContext.Request.Headers["Referer"].ToString();

            currentUrl = ViewBag.Url;
            base.OnActionExecuting(context);
        }

        private string RemoveQueryStringByKey(string url, string key)
        {
            var uri = new Uri(url);

            // this gets all the query string key value pairs as a collection
            var newQueryString = HttpUtility.ParseQueryString(uri.Query);

            // this removes the key if exists
            newQueryString.Remove(key);

            // this gets the page path from root without QueryString
            string pagePathWithoutQueryString = uri.GetLeftPart(UriPartial.Path);

            return newQueryString.Count > 0
                ? String.Format("{0}?{1}", pagePathWithoutQueryString, newQueryString)
                : pagePathWithoutQueryString;
        }
    }
}