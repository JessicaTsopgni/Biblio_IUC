﻿using System;
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
        private readonly IDocumentLogic documentLogic;

        public HomeController(IConfiguration configuration, ILoggerFactory loggerFactory,
            ICategoryLogic categoryLogic, IDocumentLogic documentLogic) : base(configuration, loggerFactory)
        {
            this.categoryLogic = categoryLogic;
            this.documentLogic = documentLogic;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<CategoryModel> categoryModels = await categoryLogic.TopCategoriesByDocument
            (
                short.Parse(configuration["DashbordCategoriesLimit"]),
                configuration["MediaFolderPath"]
            );

            IDictionary<string, long> readingOverview = await documentLogic.ReadingCountPerMonth();

            IDictionary<string, double> readingByCategoryOverview = new Dictionary<string, double>();
            var rootCategories =
            (
                await categoryLogic.GetRootsAsync(configuration["MediaFolderPath"])
            )
            .Where(x => x.DocumentIds.Count > 0).ToArray();

            foreach(var root in rootCategories)
            {
                readingByCategoryOverview.Add
                (
                    root.Name,
                    await documentLogic.ReadCountAsync(root.DocumentIds)
                );
            }
            var dashboardModel = new DashboardModel
            (
                categoryModels,
                readingOverview,
                readingByCategoryOverview
                    .Where(x => x.Value > 0)
                    .Take(short.Parse(configuration["DashbordCategoriesLimit"]))
                    .ToDictionary(x => x.Key, x => x.Value)
            );
            var pageModel = new PageModel<DashboardModel>
            (
                dashboardModel
            );
            return View(pageModel);
        }
    }
}