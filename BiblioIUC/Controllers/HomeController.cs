using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiblioIUC.Entities;
using BiblioIUC.Logics;
using BiblioIUC.Logics.Interfaces;
using BiblioIUC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BiblioIUC.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly ICategoryLogic categoryLogic;

        public HomeController(IConfiguration configuration, ILoggerFactory loggerFactory,
            ICategoryLogic categoryLogic) : base(configuration, loggerFactory)
        {
            this.categoryLogic = categoryLogic;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<CategoryModel> categoryModels = 
            (
                await
                categoryLogic.TopCategoriesByDocument
                (
                    short.Parse(configuration["DashbordCategoriesLimit"]), 
                    configuration["MediaFolderPath"]
                )
            );
            var dashboardModel = new DashboardModel(categoryModels);
            var pageModel = new PageModel<DashboardModel>
            (
                dashboardModel
            );
            return View(pageModel);
        }
    }
}