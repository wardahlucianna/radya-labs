using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.Sourcedata
{
    public class AddSourcedataRequest : CodeVm
    {
        public string IdSchool { get; set; }
        public string IdSourcedata { get; set; }
    }
}