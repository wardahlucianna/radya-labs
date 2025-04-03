using BinusSchool.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Scoring.FnScoring.StudentReflection
{
    public class ExportExcelReflectionRequest
    {
        public DateTime StartDate { set; get; }
        public DateTime EndDate { set; get; }
        public GetReflectionListResult_ReflectionData ReflectionList { set; get; }
    }
}
