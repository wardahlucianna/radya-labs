using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationUserPeriod
{
    public class GetTextbookPreparationUserPeriodResult : CodeWithIdVm
    {
        public string AcademicYear { get; set; }
        public string GroupName { get; set; }
        public string AssignAs { get; set; }
        public string OpeningDate { get; set; }
        public string ClosedDate { get; set; }
    }
}
