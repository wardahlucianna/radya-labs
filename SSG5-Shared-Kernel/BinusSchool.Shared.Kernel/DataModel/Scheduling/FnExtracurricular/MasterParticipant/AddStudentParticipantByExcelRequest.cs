using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant
{
    public class AddStudentParticipantByExcelRequest
    {
        public string IdExtracurricular { get; set; }
        public bool SendEmail { get; set; }
    }
}
