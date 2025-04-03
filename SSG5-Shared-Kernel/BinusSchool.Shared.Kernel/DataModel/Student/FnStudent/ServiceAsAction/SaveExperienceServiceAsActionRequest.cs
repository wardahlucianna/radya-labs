using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction
{
    public class SaveExperienceServiceAsActionRequest
    {
        public string IdStudent { get; set; }
        public string IdServiceAsActionForm { get; set; }
        public SaveExperienceServiceAsActionRequest_ExperienceDetail ExperienceDetail { get; set; }
        public SaveExperienceServiceAsActionRequest_SupervisorData SupervisorData { get; set; }
        public SaveExperienceServiceAsActionRequest_OrganizationDetail OrganizationDetail { get; set; }
        public List<string> IdLearningOutcomes { get; set; }
        public string NotifUrl { get; set; }
    }

    public class SaveExperienceServiceAsActionRequest_ExperienceDetail
    {
        public string IdAcademicYear { get; set; }
        public string IdServiceAsActionLocation { get; set; }
        public string ExperienceName { get; set; }
        public List<string> IdServiceAsActionTypes { get; set; }
        public List<string> IdServiceAsActionSdgs { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class SaveExperienceServiceAsActionRequest_SupervisorData
    {
        public string SupervisorName { get; set; }  
        public string SupervisorEmail { get; set; }
        public string SupervisorTitle { get; set; }
        public string SupervisorContact { get; set; }
    }

    public class SaveExperienceServiceAsActionRequest_OrganizationDetail
    {
        public string Organization { get; set; }
        public string ContributionTMC { get; set; }
        public string ActivityDescription { get; set; }
    }
}
