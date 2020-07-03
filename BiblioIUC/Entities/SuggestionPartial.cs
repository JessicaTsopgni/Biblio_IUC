using System;

namespace BiblioIUC.Entities
{
    public partial class Suggestion
    {
        public Suggestion(int id, string title, string description):this()
        {
            Id = id;
            Title = title;
            Description = description;
        }
    }
}