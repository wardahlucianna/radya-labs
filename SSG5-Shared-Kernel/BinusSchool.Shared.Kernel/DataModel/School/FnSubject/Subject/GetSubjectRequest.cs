using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.School.FnPeriod.Period;

namespace BinusSchool.Data.Model.School.FnSubject.Subject
{
    public class GetSubjectRequest : GetPeriodRequest
    {
        public string IdPathway { get; set; }
        public string IdCurriculumType { get; set; }
        public string IdSubjectGroup { get; set; }
        public IEnumerable<string> IdDepartment { get; set; }
    }
}
