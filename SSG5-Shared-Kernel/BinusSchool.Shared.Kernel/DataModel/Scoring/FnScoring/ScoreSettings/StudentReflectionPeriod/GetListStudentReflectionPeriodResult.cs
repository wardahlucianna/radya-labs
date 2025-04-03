using BinusSchool.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Scoring.FnScoring.ScoreSettings.StudentReflectionPeriod
{
    public class GetListStudentReflectionPeriodResult
    {
        public string IdStudentReflectionPeriod { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Period { get; set; }
        public string Header { get; set; }
        public string Description { get; set; }
        public int? MinCharacter { get; set; }
        public int? MaxCharacter { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LastUpdated { get; set; }
        public bool IsCanDelete { get; set; }
    }
}
