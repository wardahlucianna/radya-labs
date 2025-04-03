using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using Org.BouncyCastle.Asn1.Crmf;

namespace BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction
{
    public class GetListExperiencePerStudentResult
    {
        public ItemValueVm Student { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm OverallStatus { get; set; }
        public bool IsStatusDisabled { get; set; }
        public List<GetListExperiencePerStudentResult_Experience> ExperienceList { get; set; }
    }

    public class GetListExperiencePerStudentResult_Experience
    {
        public string IdServiceAsActionForm { get; set; }
        public string ExperienceName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ItemValueVm Location { get; set; }
        public ItemValueVm Status { get; set; }
    }
}
