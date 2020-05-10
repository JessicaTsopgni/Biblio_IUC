using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioIUC.Entities
{

    public partial class UserDocument
    {
        public UserDocument()
        {

        }

        public UserDocument(int userId, int documentId, int lastPageNumber, DateTime lastReadDate):this()
        {
            UserId = userId;
            DocumentId = documentId;
            LastPageNumber = lastPageNumber;
            LastReadDate = lastReadDate;
        }
    }
}
