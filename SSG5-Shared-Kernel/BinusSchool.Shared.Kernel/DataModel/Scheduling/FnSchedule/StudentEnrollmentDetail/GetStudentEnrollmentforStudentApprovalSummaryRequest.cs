using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollmentDetail
{
    public class GetStudentEnrollmentforStudentApprovalSummaryRequest
    {
        public string AcademicYearId { get; set; }
        public string SchoolId { get; set; }
        public string GradeId { get; set; }
        public string PathwayID { get; set; }
    }
}
