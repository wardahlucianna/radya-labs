using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.POISetting.POIMappingSettings
{
    public class GetPOIMappingSettingsDetailResult
    {
        public string IdProgrammeInq { get; set; }
        public int MaxCommentLength { get; set; }
        public int Semester { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm UnitOfInquiry { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public List<string> InfoInquiry { get; set; }
        public List<string> CentralIdea { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool IsCoTeacherCanEdit { get; set; }
    }
}
