using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioIUC.Entities
{

    public partial class UserDocument
    {
        public UserDocument()
        {

        }

        public UserDocument(long id, int userId, int documentId, int lastPageNumber, DateTime readDate):this()
        {
            Id = id;
            UserId = userId;
            DocumentId = documentId;
            LastPageNumber = lastPageNumber;
            ReadDate = readDate;
        }
    }
}
