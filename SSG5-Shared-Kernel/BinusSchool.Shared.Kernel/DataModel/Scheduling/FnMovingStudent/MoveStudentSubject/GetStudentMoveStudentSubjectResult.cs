using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentSubject
{
    public class GetStudentMoveStudentSubjectResult : CodeWithIdVm
    {
        public string IdStudent {  get; set; }
        public string IdHomeroomStudent {  get; set; }
        public string FullName {  get; set; }
        public string Level {  get; set; }
        public string Grade {  get; set; }
        public string Homeroom {  get; set; }
    }
}
