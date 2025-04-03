using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant
{
    public class GetUnselectedStudentByHomeroomRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string IdExtracurricular { get; set; }
    }
}
