using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MasterServiceAsAction.Status
{
    public class GetListServiceAsActionStatusResult : ItemValueVm
    {
        public bool? IsOverall { get; set; }
        public bool? IsDetail { get; set; }
    }
}
