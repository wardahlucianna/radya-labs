using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.StudentOperation.StudentUnderAttention
{
    public class SaveStudentUnderAttentionFutureAdmissionDecisionFormRequest
    {
        public string IdTrStudentStatus { get; set; }
        public string Reason { get; set; }
        public List<string> IdFutureAdmissionDecisionDetails { get; set; }
    }
}
