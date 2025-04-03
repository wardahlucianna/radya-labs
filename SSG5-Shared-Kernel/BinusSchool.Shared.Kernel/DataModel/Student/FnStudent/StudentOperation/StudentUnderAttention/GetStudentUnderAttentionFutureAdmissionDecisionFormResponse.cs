using BinusSchool.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.StudentOperation.StudentUnderAttention
{
    public class GetStudentUnderAttentionFutureAdmissionDecisionFormResponse
    {
        public string IdTrStudentStatus { get; set; }
        public string Reason { get; set; }
        public ItemValueVm Student { get; set; }
        public List<GetStudentUnderAttentionFutureAdmissionFormResponse_FutureAdmissionDecision> FutureAdmissionDecisions { get; set; }
    }

    public class GetStudentUnderAttentionFutureAdmissionFormResponse_FutureAdmissionDecision
    {
        public string IdFutureAdmissionDecision { get; set; }
        public string BinusUnit { get; set; }
        public bool IsMultipleAnswer { get; set; }
        public bool IsRequired { get; set; }
        public int OrderNo { get; set; }
        public List<GetStudentUnderAttentionFutureAdmissionFormResponse_FutureAdmissionDecisionDetail> FutureAdmissionDecisionDetails { get; set; }
    }

    public class GetStudentUnderAttentionFutureAdmissionFormResponse_FutureAdmissionDecisionDetail
    {
        public string IdFutureAdmissionDecisionDetail { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public bool? IsChecked { get; set; }
        public int OrderNo { get; set; }
    }
}
