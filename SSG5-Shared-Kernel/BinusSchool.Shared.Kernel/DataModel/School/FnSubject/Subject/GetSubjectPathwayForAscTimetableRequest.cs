using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSubject.Subject
{
    public class GetSubjectPathwayForAscTimetableRequest
    {
        public List<string> SubjectCode { get; set; }
        public List<string> GradeCode { get; set; }
        public string IdSchool { get; set; }
     
    }
}
