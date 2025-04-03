using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;

namespace BinusSchool.Shared.Kernel.DataModel.Scoring.FnScoring.StudentReflection
{
    public class GetStudentReflectionEntryPeriodResult
    {
        public bool IsInPeriod { set; get; }
        public string IdStudentReflectionPeriod { set; get; }
        public string Header { set; get; }
        public string Description { set; get; }
        public int? MinCharacter { get; set; }
        public int? MaxCharacter { set; get; }
        public DateTime StartDate { set; get; }
        public DateTime EndDate { set; get; }
        public CodeWithIdVm AcademicYear { set; get; }
        public ItemValueVm Semester { set; get; }
        public ItemValueVm Term { set; get; }
    }
}
