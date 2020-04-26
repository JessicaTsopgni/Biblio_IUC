using BiblioIUC.Localize;
using BiblioIUC.Logics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioIUC.Models
{
    public class LayoutModel
    {
        public string SearchValue { get; set; }
        public string SearchPlaceHolder { get; set; }
        public string SearchAction { get; set; }
        public string SearchController { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string ReturnUrl { get; set; }

        public LayoutModel()
        {
            SearchPlaceHolder = Text.Searh_a_document;
            SearchAction = "Index";
            SearchController = "Document";
            PageIndex = DocumentLogic.DEFAULT_PAGE_INDEX;
            PageSize = DocumentLogic.DEFAULT_PAGE_SIZE;
        }

        public LayoutModel(string returnUrl) : this()
        {
            ReturnUrl = returnUrl;
        }

        public LayoutModel(string returnUrl, string searchValue, int pageIndex, int pageSize):this(returnUrl)
        {
            SearchValue = searchValue;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }

        public LayoutModel(string returnUrl, string searchValue, int pageIndex, int pageSize,
            string searchPlaceHolder, string searchAction, string searchController)
            :this(returnUrl, searchValue, pageIndex, pageSize )
        {
            SearchValue = searchValue;
            PageIndex = pageIndex;
            PageSize = pageSize;
            SearchPlaceHolder = searchPlaceHolder;
            SearchAction = searchAction;
            SearchController = searchController;
        }


        public LayoutModel(LayoutModel layoutModel) : this(layoutModel?.ReturnUrl,
            layoutModel?.SearchValue, layoutModel?.PageIndex ?? CategoryLogic.DEFAULT_PAGE_INDEX, 
            layoutModel?.PageSize ??  CategoryLogic.DEFAULT_PAGE_SIZE, layoutModel ?.SearchPlaceHolder,
            layoutModel ?.SearchAction, layoutModel ?.SearchController)
        {
           
        }

    }
}
