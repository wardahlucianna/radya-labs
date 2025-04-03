using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scoring.FnScoring.PrivilegeEntryScore
{
    public class GetPrivilegeEntryScoreByPositionRequest 
    {
        public string IdAcademicYear { get; set; }
        public string IdRole { get; set; }
        public List<string> PositionShortName { get; set; }
        public string IdUser { get; set; }
    }
}
