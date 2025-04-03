using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSubject.Subject
{
    public class GetSubjectPathwayForAscTimetableResult
    {
        public string IdSubject { get; set; }
        public string IdGrade { get; set; }
        public string GradeCode { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectDescription { get; set; }
        public List<SubjectLevelVM> SubjectLevel { get; set; }
   
    }

    public class SubjectLevelVM
    {
        public string IdSubjectLevel { get; set; }
        public string SubjectlevelCode { get; set; }
        public string SubjectlevelDesc { get; set; }
        public bool IsDefault { get; set; }
    }
}
