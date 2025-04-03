using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ProgressStatus
{
    public class ApprovalProgressStatusRequest
    {
        public List<ApprovalProgressStatusRequest_Body> ApprovalList { get; set; }
    }

    public class ApprovalProgressStatusRequest_Body
    {
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public string IdUpdateStudentProgressStatus { get; set; }
        public string Comment { get; set; }
        public ApprovalStatus Action { get; set; }
    }
}
