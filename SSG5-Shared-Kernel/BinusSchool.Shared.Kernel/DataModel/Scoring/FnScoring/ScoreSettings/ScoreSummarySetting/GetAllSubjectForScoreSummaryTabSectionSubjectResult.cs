using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreSummarySetting
{
    public class GetAllSubjectForScoreSummaryTabSectionSubjectResult : CodeWithIdVm
    {
        public List<GetAllSubjectForScoreSummaryTabSectionSubjectResult_Subject> Subjects { get; set; }
    }

    public class GetAllSubjectForScoreSummaryTabSectionSubjectResult_Subject
    {
        public string IdSubject { get; set; }
        public string SubJectID { get; set; }
        public string SubjectName { get; set; }
        public int OrderNumber { get; set; }
        public bool IsCheck { get; set; }
    }
}
