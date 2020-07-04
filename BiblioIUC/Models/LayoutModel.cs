using BiblioIUC.Localize;
using BiblioIUC.Logics;
using BiblioIUC.Logics.Interfaces;
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
        public int UnReadedSuggestionCount { get; set; }
        public IEnumerable<SuggestionModel> SuggestionModels { get; set; }

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

        public LayoutModel(string returnUrl, string searchValue, int pageIndex, int pageSize,
            int unReadedSuggestionCount, IEnumerable<SuggestionModel> suggestionModels) :this(returnUrl)
        {
            SearchValue = searchValue;
            PageIndex = pageIndex;
            PageSize = pageSize;
            UnReadedSuggestionCount = unReadedSuggestionCount;
            SuggestionModels = suggestionModels;
        }

        public LayoutModel(string returnUrl, string searchValue, int pageIndex, int pageSize,
            int unReadedSuggestionCount, IEnumerable<SuggestionModel> suggestionModels,
            string searchPlaceHolder, string searchAction, string searchController)
            :this(returnUrl, searchValue, pageIndex, pageSize, unReadedSuggestionCount, suggestionModels)
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
            layoutModel?.PageSize ??  CategoryLogic.DEFAULT_PAGE_SIZE,
            layoutModel?.UnReadedSuggestionCount ?? 0, layoutModel?.SuggestionModels,
            layoutModel ?.SearchPlaceHolder,
            layoutModel ?.SearchAction, layoutModel ?.SearchController)
        {
           
        }

        public async Task Refresh(ISuggestionLogic suggestionLogic, int limit , string mdiaFolderPath)
        {
            UnReadedSuggestionCount = await suggestionLogic.UnReadCountAsync();
            SuggestionModels = await suggestionLogic.TopSuggestions(limit, mdiaFolderPath);
        }

    }
}
