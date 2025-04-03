using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.ParamGlobal
{
    public class SelectManualRequest : CollectionRequest
    {
        public SelectManualRequest()
        {
            GetAll = true;
            Key = string.Empty;
        }

        public string Key { get; set; }
    }
}
