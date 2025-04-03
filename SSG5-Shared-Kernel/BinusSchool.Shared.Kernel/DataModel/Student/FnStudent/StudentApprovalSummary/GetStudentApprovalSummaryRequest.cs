using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentApprovalSummary
{
    public class GetStudentApprovalSummaryRequest : CollectionRequest
    {
        public string AcademicYear { get; set; }
        public string SchoolId { get; set; }
        public string HomeroomId { get; set; }
        public string GradeId { get; set; }
        public string LevelId { get; set; }
        public int Semester { get; set; }
    }
}
