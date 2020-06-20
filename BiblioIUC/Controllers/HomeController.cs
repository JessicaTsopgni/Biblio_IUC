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
        private readonly IDocumentLogic documentLogic;

        public HomeController(IConfiguration configuration, ILoggerFactory loggerFactory,
            ICategoryLogic categoryLogic, IDocumentLogic documentLogic) : base(configuration, loggerFactory)
        {
            this.categoryLogic = categoryLogic;
            this.documentLogic = documentLogic;
        }

        public async Task<IActionResult> Index()
        {
            var dashboardTopCategoryReadingsLimit = int.Parse(configuration["DashboardTopCategoryReadingsLimit"]);
            var dashboardTopSubCategoryReadingsLimit = int.Parse(configuration["DashboardTopSubCategoryReadingsLimit"]);
            var dashboardTopPercentSubCategoryReadingsLimit = int.Parse(configuration["DashboardTopPercentSubCategoryReadingsLimit"]);
            var dashboardTopDocumentReadingsLimit = int.Parse(configuration["DashboardTopDocumentReadingsLimit"]);

            //top sub categories reading
            IDictionary<CategoryModel, double> topSubCategoriesReading = await categoryLogic.TopSubCategoriesReading
            (
                dashboardTopSubCategoryReadingsLimit,
                configuration["MediaFolderPath"]
            );

            //reading per month
            IDictionary<string, long> readingOverview = await documentLogic.ReadingCountPerMonth();

            //top categories readings
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

            // top document reading limit 
            var topDocumentReadings = await documentLogic.TopReading(dashboardTopDocumentReadingsLimit, configuration["MediaFolderPath"]);


            var dashboardModel = new DashboardModel
            (
                readingByCategoryOverview.Count < dashboardTopCategoryReadingsLimit ? readingByCategoryOverview.Count : dashboardTopCategoryReadingsLimit,
                dashboardTopSubCategoryReadingsLimit,
                topSubCategoriesReading.Count < dashboardTopPercentSubCategoryReadingsLimit ? topSubCategoriesReading.Count : dashboardTopPercentSubCategoryReadingsLimit,
                topSubCategoriesReading,
                readingOverview,
                readingByCategoryOverview
                    .Where(x => x.Value > 0)
                    .OrderByDescending(x => x.Value)
                    .Take(dashboardTopCategoryReadingsLimit)
                    .ToDictionary(x => x.Key, x => x.Value),
                topDocumentReadings
            );
            
            var pageModel = new PageModel<DashboardModel>
            (
                dashboardModel
            );
            return View(pageModel);
        }
    }
}