using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.BLPGroup
{
    public class GetBLPGroupDetailResult
    {
        public ItemValueVm AcademicYear { get; set; }
        //public NameValueVm Semester { get; set; }
        public ItemValueVm Level { get; set; }
        public string GroupName { get; set; }
    }
}
