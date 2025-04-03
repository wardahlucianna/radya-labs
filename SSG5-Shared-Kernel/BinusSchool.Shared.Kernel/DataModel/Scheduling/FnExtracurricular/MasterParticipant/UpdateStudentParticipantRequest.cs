using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant
{
    public class UpdateStudentParticipantRequest
    {
        public string IdHomeroom { get; set; }
        public string IdExtracurricular { get; set; }
        public string IdStudent { get; set; }
        public bool Status { get; set; }
    }
}
