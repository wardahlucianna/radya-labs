using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularUnattendance
{
    public class AddExtracurricularUnattendanceRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public DateTime UnattendanceStartDate { get; set; }
        public DateTime UnattendanceEndDate { get; set; }
        public string Description { get; set; }
        public List<UnattendanceExtracurricular> UnattendanceExtracurricularList { get; set; }
    }

    public class UnattendanceExtracurricular
    {
        public string IdExtracurricular { get; set; }
    }
}
