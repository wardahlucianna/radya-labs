using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentScoreSummaryByHomeroom
{
    public class GetStudentScoreViewHeaderByHomeroomResult
    {
        public List<SubjectTypeVm> SubjectTypeList { set; get; }
    }

    public class SubjectViewHomeroomHeaderVm
    {
        public string IdSubjectType { set; get; }
        public string SubjectTypeName { set; get; }
        public string IdSubject { set; get; }
        public string SubjectName { set; get; }
        public string IdSubjectLevel { set; get; }
    }

}
