using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;

namespace BinusSchool.Data.Model.School.FnSchool.SurveySummary
{
    public class GetSurveySummaryUserRespondentResult
    {
        public string IdUser { get; set; }
        public string IdUserChild { get; set; }
        public string IdParent { get; set; }
        public string IdHomeroomStudent { get; set; }
        public ItemValueVmWithOrderNumber Level { get; set; }
        public ItemValueVmWithOrderNumber Grade { get; set; }
        public SurveySummaryUserRespondentHomeroom Homeroom { get; set; }
        public string IdPusblishSurvey { get; set; }
        public string Role { get; set; }
    }

    public class SurveySummaryUserRespondentHomeroom : ItemValueVm
    {
        public int? Semester { get; set; }
    }
}
