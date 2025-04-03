using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant
{
    public class DeleteStudentParticipantResult
    {
        public string IdStudent { get; set; }
        public string IdExtracurricular { get; set; }
        public string IdHomeroom { get; set; }
        public bool IsSuccess { get; set; }
    }
}
