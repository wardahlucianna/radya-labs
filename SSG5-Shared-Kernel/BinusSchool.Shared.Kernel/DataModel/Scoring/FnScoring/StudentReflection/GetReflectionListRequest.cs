using BinusSchool.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Scoring.FnScoring.StudentReflection
{
    public class GetReflectionListRequest
    {
        public string IdUser { set; get; }
        public string IdAcademicYear { set; get; }
        public int Semester { set; get; }
    }
}
