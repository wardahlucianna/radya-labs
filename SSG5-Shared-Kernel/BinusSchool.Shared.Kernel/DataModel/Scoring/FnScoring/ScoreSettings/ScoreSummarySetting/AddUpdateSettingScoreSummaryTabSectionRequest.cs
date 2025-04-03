using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreSummarySetting
{
    public class AddUpdateSettingScoreSummaryTabSectionRequest
    {
        public string? IdScoreSummaryTabSection { get; set; }
        public string IdScoreSummaryTab { get; set; }
        public string? IdSubjectType { get; set; }
        public int OrderNumber { get; set; }
        public string ScoreTabTemplate { get; set; }
        public string WidthColumn { get; set; }
        public bool ShowTeacherName { get; set; }
        public bool ShowTotal { get; set; }
        public bool ShowSubjectLevel { get; set; }
        public bool HideInSemesterOne { get; set; }
        public string Content { get; set; }
        public string MobileContent { get; set; }
        
        public List<AddUpdateSettingScoreSummaryTabSectionRequest_Subject> Subjects { get; set; }
    }

    public class AddUpdateSettingScoreSummaryTabSectionRequest_Subject
    {
        public string SubjectID { get; set; }
        public int OrderNumber { get; set; }
    }
}
