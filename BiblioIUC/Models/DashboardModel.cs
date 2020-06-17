using BiblioIUC.Entities;
using BiblioIUC.Localize;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioIUC.Models
{
    public class DashboardModel:BaseModel
    {
        public IEnumerable<CategoryModel> CategoryModels { get; set; }
        public IDictionary<string, long> ReadingsOverview { get; set; }

        public DashboardModel(IEnumerable<CategoryModel> categoryModels,
            IDictionary<string, long> readingsOverview) :base()
        {
            CategoryModels = categoryModels;
            ReadingsOverview = readingsOverview;
        }
    }
}
