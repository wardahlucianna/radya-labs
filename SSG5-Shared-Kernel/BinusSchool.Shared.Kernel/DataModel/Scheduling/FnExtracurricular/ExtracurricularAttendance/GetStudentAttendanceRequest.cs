using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class GetStudentAttendanceRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdExtracurricular { get; set; }
        public Month Month { get; set; }
    }
}
