using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant
{
    public class ExportExcelStudentParticipantRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public List<string> IdExtracurricular { get; set; }
    }
}
