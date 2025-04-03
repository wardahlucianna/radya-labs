using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnBlocking.StudentBlocking
{
    public class GetContentDataStudentBlockingRequest : CollectionRequest
    {
        public string IdUser { get; set; }
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string Semester { get; set; }
        public string IdHoomRoom { get; set; }
        public string IdStudent { get; set; }
    }
}
