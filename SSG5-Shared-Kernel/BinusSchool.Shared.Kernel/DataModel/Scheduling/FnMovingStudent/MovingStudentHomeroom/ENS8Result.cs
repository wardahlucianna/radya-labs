using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MovingStudentHomeroom
{
    public class ENS8Result
    {
        public List<string> IdUserRecepient { get; set; }
        public List<string> IdUserCC { get; set; }
        public List<ENS8MoveHomeroom> MoveHomeroom { get; set; }
    }

    public class ENS8MoveHomeroom
    {
        public string AcademicYear { get; set; }
        public string Semester { get; set; }
        public string OldHomeroom { get; set; }
        public string NewHomeroom { get; set; }
        public string StudentName { get; set; }
        public string EffectiveDate { get; set; }
        public string Notes { get; set; }
    }
}
