using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;


namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetUserByRolePositionExcludeSubjectRequest
    {
        public string IdRole { get; set; }
        public string CodePosition { get; set; }
        public string IdAcademicYear { get; set; }
        public List<string> IdGradeParticipants { get; set; }
        public List<string> ExcludeIdSubject { get; set; }
    }
}
