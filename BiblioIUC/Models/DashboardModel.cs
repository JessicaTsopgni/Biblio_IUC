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

        public IEnumerable<CategoryModel> TopSubCategoryReadings { get; set; }
        public IDictionary<string, long> ReadingsPerMonth { get; set; }
        public IDictionary<CategoryModel, double> TopPercentBySubCategoryReadings { get; set; }
        public IDictionary<string, double> TopCategoryReadings { get; set; }
        public string ReadingsOverviewTitle { get; set; }
        public string ReadingsByCategoryOverviewTitle { get; set; }
        public string ReadingsBySubCategoryOverviewTitle { get; set; }
        public IEnumerable<DocumentModel> TopDocumentReadings { get; set; }
        public DashboardModel(int readingsByCategoryOverviewLimit,
            int readingsBySubCategoryOverviewLimit,
            int readingsPercentBySubCategoryOverviewLimit,
            IDictionary<CategoryModel, double> topPercentBySubCategoryReadings,
            IDictionary<string, long> readingsPerMonth,
            IDictionary<string, double> topCategoryReadings,
            IEnumerable<DocumentModel> topDocumentReadings) :base()
        {
            ReadingsOverviewTitle = Text.Readings_overview;
            ReadingsByCategoryOverviewTitle = string.Format(Text.Top_x_of_readings_by_category, readingsByCategoryOverviewLimit);
            ReadingsBySubCategoryOverviewTitle = string.Format(Text.Top_x_of_readings_by_sub_category, readingsPercentBySubCategoryOverviewLimit);

            TopPercentBySubCategoryReadings = topPercentBySubCategoryReadings.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            TopSubCategoryReadings = topPercentBySubCategoryReadings.Keys.Take(readingsBySubCategoryOverviewLimit).ToArray();
            ReadingsPerMonth = readingsPerMonth;
            TopCategoryReadings = topCategoryReadings;
            TopDocumentReadings = topDocumentReadings;
        }
    }
}
