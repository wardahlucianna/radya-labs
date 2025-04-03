using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MovingStudentHomeroom
{
    public class GetStudentMoveStudentHomeroomResult
    {
        public string IdStudent { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string FullName { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public bool IsCanMove { get; set; }
    }
}
