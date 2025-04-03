using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class AddExperienceRequest
    {
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
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
        public List<string> IdLearningOutcomes { get; set; }
    }
}
