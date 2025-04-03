using System;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Pkcs;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class GetDownloadLessonPlanSummaryRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public string PositionCode { get; set; }
        public bool IsGrade { get; set; }
        public bool IsSubject { get; set; }
        public List<string> IdGrade { get; set; }
        public List<string> IdSubject { get; set; }
    }
}
