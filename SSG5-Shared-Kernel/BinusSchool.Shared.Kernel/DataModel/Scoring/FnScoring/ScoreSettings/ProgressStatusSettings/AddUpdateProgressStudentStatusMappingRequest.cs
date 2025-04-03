using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ProgressStatusSettings
{
    public class AddUpdateProgressStudentStatusMappingRequest
    {
        public string IdAcademicYear { set; get; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public bool NeedApproval { set; get; }
        public string IdApprovalWorkflow { set; get; }
        public List<AddUpdateProgressStudentStatusMappingRequest_Mapping> Mapping { set; get; }
        public List<string> DeleteMapping { set; get; }
    }

    public class AddUpdateProgressStudentStatusMappingRequest_Mapping
    {
        public string? IdProgressStudentStatusMapping { set; get; }
        public string IdGrade { set; get; }
        public int? IdStudentStatus { set; get; }
        public string StudentProgressText { set; get; }
        public string StudentProgressBahasaText { set; get; }
        public bool EnableAgreement { set; get; }
        public bool EnableHideReportCard { set; get; }
    }

    public class AddUpdateProgressStudentStatusMappingRequest_ProgressStatusDesc
    {
        public string? ProgressStatusDesc { set; get; }
        public string? ProgressStatusBahasaDesc { set; get; }
    }
}
