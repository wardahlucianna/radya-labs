using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation
{
    public class GetAvailabilityTimeTeacherRequest
    {
        public string IdUserTeacher { get; set; }
        public string IdAcademicYear { get; set; }
        public DateTime DateInvitation { get; set; }
    }
}
