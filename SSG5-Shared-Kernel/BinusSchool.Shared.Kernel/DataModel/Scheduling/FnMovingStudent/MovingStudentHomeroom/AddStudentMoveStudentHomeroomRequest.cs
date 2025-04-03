using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MovingStudentHomeroom
{
    public class AddStudentMoveStudentHomeroomRequest
    {
        public string IdAcademicYear {  get; set; }
        public int Semester {  get; set; }
        public string IdHomeroomOld {  get; set; }
        public string IdHomeroom {  get; set; }
        public DateTime EffectiveDate {  get; set; }
        public bool IsSendEmail { get; set; }
        public string Note {  get; set; }
        public List<string> IdStudents { get; set; }
    }
}
