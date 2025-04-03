using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.ChildStatus
{
    public class GetChildStatusRequest : CollectionRequest
    {
        public string IdChildStatus { get; set; }
    }
}