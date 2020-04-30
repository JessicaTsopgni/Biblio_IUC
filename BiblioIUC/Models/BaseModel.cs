using BiblioIUC.Localize;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioIUC.Models
{
    public class BaseModel
    {
        public int Id { get; set; }

        public string ImageLink { get; protected set; }

        public IFormFile ImageUploaded { get; set; }

        [Display(Name = "Delete_this_image_on_save", ResourceType = typeof(Text))]
        public bool DeleteImage { get; set; }
        public StatusOptions Status { get; set; }

        public string StatusName
        {
            get
            {
                string text = "";
                switch (Status)
                {
                    case StatusOptions.Ended:
                        text = Text.Ended;
                        break;
                    case StatusOptions.Actived:
                        text = Text.Actived;
                        break;
                    case StatusOptions.Disabled:
                        text = Text.Disabled;
                        break;
                    case StatusOptions.InProcess:
                        text = Text.InProcess;
                        break;
                    case StatusOptions.Deleted:
                        text = Text.Deleted;
                        break;
                }
                return text;
            }
        }

        [Display(Name = "Status", ResourceType = typeof(Text))]
        public bool State { get; set; }


        public BaseModel() { }

        public BaseModel(int id, StatusOptions status):this()
        {
            Id = id;
            Status = status;
            State = status == StatusOptions.Actived;
        }

    }
}
