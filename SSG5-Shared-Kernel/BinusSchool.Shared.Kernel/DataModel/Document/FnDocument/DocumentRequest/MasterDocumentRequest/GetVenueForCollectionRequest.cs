using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class GetVenueForCollectionRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
    }
}
