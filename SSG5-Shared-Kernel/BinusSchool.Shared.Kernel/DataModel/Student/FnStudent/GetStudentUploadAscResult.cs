using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent
{
    public class GetStudentUploadAscResult
    {
        public string IdStudent{get;set;}
        public string FullName{get;set;}
        public string BinusianId{get;set;}
        public string Gender{get;set;}
        public string Religion{get;set;}
        public string ReligionSubject{get;set;}
        public string IdSchool { get; set; }
        public List<string> IdGrade { get; set; }
    }
}
