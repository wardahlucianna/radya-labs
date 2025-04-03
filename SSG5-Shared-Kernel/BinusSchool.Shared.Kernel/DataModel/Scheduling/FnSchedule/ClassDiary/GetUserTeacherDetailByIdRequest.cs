using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary
{
    public class GetUserTeacherDetailByIdRequest
    {
        public string IdAcademicYear { get; set; }
        public string UserId { get; set; }
        public int? Semester { get; set; }
    }
}
