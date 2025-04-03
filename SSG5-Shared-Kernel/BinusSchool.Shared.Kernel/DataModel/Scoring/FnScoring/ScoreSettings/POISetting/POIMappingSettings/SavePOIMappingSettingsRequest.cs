using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using NPOI.OpenXml4Net.OPC.Internal;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.POISetting.POIMappingSettings
{
    public class SavePOIMappingSettingsRequest
    {
        public string IdProgrammeInq { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdUnitOfInquiry { get; set; }
        public List<string> IdGrades { get; set; }
        public int MaxCommentLength { get; set; }
        public List<string> IdCentralIdeas { get; set; }
        public List<string> IdInfoUnits { get; set; }
        public DateTime StartDate { get;set;}
        public DateTime EndDate { get; set; }
        public bool IsCoTeacherCanEdit { get; set; }
    }
}
