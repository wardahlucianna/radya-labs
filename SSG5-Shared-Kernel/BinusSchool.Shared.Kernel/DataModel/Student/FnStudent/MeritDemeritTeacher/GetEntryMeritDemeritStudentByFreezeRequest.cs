using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class GetEntryMeritDemeritStudentByFreezeRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }  
        public string IdLevel { get; set; }  
        public string IdGrade { get; set; }  
        public string IdHomeroom { get; set; }
        public string Semester { get; set; }
        public ICollection<string> ExcludeStudents { get; set; }
        public DateTime Date { get; set; }

    }
}
