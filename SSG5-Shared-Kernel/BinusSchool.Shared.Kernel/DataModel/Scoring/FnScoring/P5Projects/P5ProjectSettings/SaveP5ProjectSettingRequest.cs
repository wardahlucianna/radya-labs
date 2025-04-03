using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.P5Projects.P5ProjectSettings
{
    public class SaveP5ProjectSettingRequest
    {
        public string IdP5Project { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public List<string> Grades { get; set; }
        public string IdP5Theme { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> IdP5ProjectOutcomes { get; set; }
        public List<string> DeletedP5ProjectOutcomes { get; set; }
        public List<string> PIC { get; set; }
    }
}
