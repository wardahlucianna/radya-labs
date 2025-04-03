using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary
{
    public class GetHomeroomByStudentRequest
    {
        public string AcademicYearId { get; set; }
        public string GradeId { get; set; }
        public string SubjectId { get; set; }
        public string UserId { get; set; }
        public int Semester { get; set; }
    }
}
