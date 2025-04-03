using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class DetailExperienceResult
    {
        public string Id { get; set; }
        public string IdStudent { get; set; }
        public string IdHomeroomStudent { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public string ExperienceName { get; set; }
        public List<ExperienceType> ExperienceType { get; set; }
        public ExperienceLocation ExperienceLocation { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdUserSupervisor { get; set; }
        public string SupervisorName { get; set; }
        public string RoleName { get; set; }
        public string PositionName { get; set; }
        public string SupervisorTitle { get; set; }
        public string SupervisorEmail { get; set; }
        public string SupervisorContact { get; set; }
        public string Organization { get; set; }
        public string Description { get; set; }
        public string ContributionOrganizer { get; set; }
        public List<CodeWithIdVm> LearningOutcomes { get; set; }
        public ExperienceStatus Status { get; set; }
        public bool CanEdit { get; set; }
        public List<ListApproverNote> Approver { get; set; }

    }

    public class ListApproverNote
    {
        public string ApproverDate { get; set; }
        public string ApproverName { get; set; }
        public string ApproverNote { get; set; }
    }
}
