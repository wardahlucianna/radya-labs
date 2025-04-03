using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant
{
    public class AddStudentParticipantRequest
    {
        public string IdHomeroom { get; set; }
        public string IdExtracurricular { get; set; }
        public string IdStudent { get; set; }
        public decimal? ExtracurricularPrice { get; set; }
        public DateTime JoinDate { get; set; }
        public bool SendEmailNotification { get; set; }
    }
}
