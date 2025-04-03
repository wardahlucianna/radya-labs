using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Scoring.FnScoring.ScoreSettings.StudentReflectionPeriod
{
    public class GetListStudentReflectionPeriodRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
    }
}
