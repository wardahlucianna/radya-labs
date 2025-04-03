using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Scoring.FnScoring.ScoreSettings.StudentReflectionPeriod
{
    public class SaveStudentReflectionPeriodRequest
    {
        public string IdStudentReflectionPeriod { get; set; }
        public int Semester { get; set; }
        public string IdLevel { get; set; }
        public List<string> IdGrades { get; set; }
        public string Header { get; set; }
        public string Description { get; set; }
        public int? MinCharacter { get; set; }
        public int? MaxCharacter { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
