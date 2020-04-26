using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioIUC.Models
{
    public class PageModel<T>:LayoutModel
    {
        public T DataModel { get; set; }

        public PageModel() : base() 
        {
        }


        public PageModel(T dataModel)
            : base()
        {
            DataModel = dataModel;
        }

        public PageModel(T dataModel, LayoutModel layoutModel)
            : base(layoutModel)
        {
            DataModel = dataModel;
        }


    }
}
