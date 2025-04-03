using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using Org.BouncyCastle.Asn1.Mozilla;

namespace BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction
{
    public class GetServiceAsActionDetailFormResult
    {
        public ItemValueVm Student { get; set; }    
        public CodeWithIdVm Grade { get; set; }
        public ItemValueVm Classroom { get; set; }
        public GetServiceAsActionDetailFormResult_StatusDetail StatusDetail { get; set; }
        public GetServiceAsActionDetailFormResult_ExperienceDetail ExperienceDetail { get; set; }
        public GetServiceAsActionDetailFormResult_SupervisorData? SupervisorData { get; set; }
        public GetServiceAsActionDetailFormResult_OrganizationDetail OrganizationDetail { get; set; }
        public List<ItemValueVm> LearningOutcomes { get; set; }
        public List<GetServiceAsActionDetailFormResult_Evidence>? Evidences { get; set; }
    }

    public class GetServiceAsActionDetailFormResult_StatusDetail
    {
        public ItemValueVm Status { get; set; }
        public GetServiceAsActionDetailFormResult_StatusDetail_Revision? Revision { get; set; }
        public bool CanChangeStatus { get; set; }
    }

    public class GetServiceAsActionDetailFormResult_StatusDetail_Revision
    {
        public GetServiceAsActionDetailFormResult_StatusDetail_Revision_Approver? Approver { get; set; }
        public DateTime? Date { get; set; }
        public string? Note { get; set; }
    }

    public class GetServiceAsActionDetailFormResult_StatusDetail_Revision_Approver
    {
        public string? Id { get;set; }
        public string? Description { get; set; }
    }

    public class GetServiceAsActionDetailFormResult_ExperienceDetail
    {
        public ItemValueVm AcademicYear { get; set; }
        public string ExperienceName { get; set; }
        public ItemValueVm ExperienceLocation { get; set; }
        public List<ItemValueVm> ExperienceType { get; set; }
        public List<ItemValueVm> ExperienceSdgs { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool CanAddEvidence { get; set; }
    }

    public class GetServiceAsActionDetailFormResult_SupervisorData
    {
        public ItemValueVm? Supervisor { get; set; }
        public string SupervisorEmail { get; set; }
        public string SupervisorTitle { get; set; }
        public string SupervisorContact { get; set; }
    }

    public class GetServiceAsActionDetailFormResult_OrganizationDetail
    {
        public string Organization { get; set; }
        public string ContributionTMC { get; set; }
        public string ActivityDescription { get; set; } 
    }

    public class GetServiceAsActionDetailFormResult_Evidence
    {
        public string IdServiceAsActionEvidence { get; set; }
        public DateTime? Datein { get; set; }
        public string EvidenceType { get; set; }
        public bool CanEditEvidence { get; set; }
        public bool CanDeleteEvidence { get; set; }
        public bool CanAddComment { get; set; }
        public string EvidenceText { get; set; }
        public string EvidenceURL { get; set; }
        public List<GetServiceAsActionDetailFormResult_Evidence_Uploads>? Urls { get; set; }
        public List<ItemValueVm> LearningOutcomes { get; set; }
        public List<GetServiceAsActionDetailFormResult_Evidence_Comment>? Comments { get; set; }
    }

    public class GetServiceAsActionDetailFormResult_Evidence_Uploads
    {
        public string EvidenceFIGM { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string FileType { get; set; }
    }

    public class GetServiceAsActionDetailFormResult_Evidence_Comment
    {
        public string IdServiceAsActionComment { get; set; }
        public ItemValueVm Commentator { get; set; }
        public string Comment { get; set; }
        public DateTime? CommentDate { get; set; }
        public bool CanDeleteComment { get; set; }
        public bool CanEditComment { get; set; }
    }
}
