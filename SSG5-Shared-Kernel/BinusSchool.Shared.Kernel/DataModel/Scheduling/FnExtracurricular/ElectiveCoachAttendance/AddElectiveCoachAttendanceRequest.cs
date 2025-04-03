using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoachAttendance
{
    public class AddElectiveCoachAttendanceRequest 
    {
        public string IdAcademicYear { set; get; }
        public int Semester { set; get; }
        public string IdExternalCoach { set; get; }
    }
}
