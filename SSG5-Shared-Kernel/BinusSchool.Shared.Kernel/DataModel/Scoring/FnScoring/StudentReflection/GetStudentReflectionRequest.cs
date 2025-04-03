using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Scoring.FnScoring.StudentReflection
{
    public class GetStudentReflectionRequest
    {
        public string IdStudent { set; get; }
        public string IdStudentReflectionPeriod { set; get; }
    }
}
