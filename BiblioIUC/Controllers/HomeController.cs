using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiblioIUC.Entities;
using BiblioIUC.Logics;
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
        public HomeController(IConfiguration configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {

        }
        public IActionResult Index()
        {
            var dashboardModel = new DashboardModel();
            var pageModel = new PageModel<DashboardModel>
            (
                dashboardModel
            );
            return View(pageModel);
        }
    }
}