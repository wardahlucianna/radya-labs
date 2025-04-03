using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Scoring.FnScoring.StudentReflection
{
    public class GetStudentReflectionEntryPeriodRequest
    {
        public DateTime CurrentDate { set; get; }
        public string IdSchool { set; get; }
        public string IdStudent { set; get; }
    }
}
