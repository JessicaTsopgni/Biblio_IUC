using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioIUC.Models
{
    public class PageListModel<T>:LayoutModel
    {
        public IEnumerable<T> DataModels { get; set; }

        public PageListModel():base()
        {

        }

        public PageListModel(IEnumerable<T> dataModels, LayoutModel layoutModel)
            :base(layoutModel)
        {
            DataModels = dataModels;
        }

        public PageListModel(IEnumerable<T> dataModels, string returnUrl, 
            string searchValue, int pageIndex, int pageSize,
            int unReadedSuggestionCount, IEnumerable<SuggestionModel> suggestionModels)
          : base(returnUrl, searchValue, pageIndex, pageSize, unReadedSuggestionCount, suggestionModels)
        {
            DataModels = dataModels;
        }

        public PageListModel(IEnumerable<T> dataModels, string returnUrl, string searchValue,
            int pageIndex, int pageSize,
            int unReadedSuggestionCount, IEnumerable<SuggestionModel> suggestionModels,
          string searchPlaceHolder, string searchAction, string searchController)
          : base(returnUrl, searchValue, pageIndex, pageSize,unReadedSuggestionCount, 
                suggestionModels, searchPlaceHolder, 
                searchAction, searchController )
        {
            DataModels = dataModels;
        }
    }
}
