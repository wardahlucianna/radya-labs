using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularHistory
{
    public class GetStudentExtracurricularHistoryListRequest
    {
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public int? Semester { get; set; }
    }
}
