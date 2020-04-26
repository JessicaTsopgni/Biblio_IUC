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

        public PageListModel(IEnumerable<T> dataModels, string returnUrl, string searchValue, int pageIndex, int pageSize)
          : base(returnUrl, searchValue, pageIndex, pageSize)
        {
            DataModels = dataModels;
        }

        public PageListModel(IEnumerable<T> dataModels, string returnUrl, string searchValue, int pageIndex, int pageSize,
          string searchPlaceHolder, string searchAction, string searchController)
          : base(returnUrl, searchValue, pageIndex, pageSize, searchPlaceHolder, searchAction, searchController)
        {
            DataModels = dataModels;
        }
    }
}
