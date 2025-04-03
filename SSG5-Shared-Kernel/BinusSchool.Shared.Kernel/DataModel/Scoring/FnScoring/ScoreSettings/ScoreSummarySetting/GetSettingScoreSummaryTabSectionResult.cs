using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreSummarySetting
{
    public class GetSettingScoreSummaryTabSectionResult
    {
        public string IdScoreSummaryTabSection { get; set; }
        public int OrderNumber { get; set; }
        public ItemValueVm ScoreTabTemplate { get; set; }
        public ItemValueVm WidthColumn { get; set; }
        public bool ShowTeacherName { get; set; }
        public bool ShowTotal { get; set; }
        public bool ShowSubjectLevel { get; set; }
        public bool HideInSemesterOne { get; set; }
        public string Content { get; set; }
        public string MobileContent { get; set; }
        public ItemValueVm? SubjectType { get; set; }
        public List<ItemValueVm> Subjects { get; set; }
    }
}
