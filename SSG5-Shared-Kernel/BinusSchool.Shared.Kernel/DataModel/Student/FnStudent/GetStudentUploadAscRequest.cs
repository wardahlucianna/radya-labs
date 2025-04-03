using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent
{
    public class GetStudentUploadAscRequest
    {
        public List<string> BinusianId { get; set; }
        public string IdSchool { get; set; }
        public string IdGrade { get; set; }
    }
}
