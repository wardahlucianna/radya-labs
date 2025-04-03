using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Scoring.FnScoring.StudentReflection
{
    public class GetStudentReflectionResult
    {
        public string IdStudentReflection { set; get; }
        public string Reflection { set; get; }
        public DateTime? LastSubmit { set; get; }
    }
}
