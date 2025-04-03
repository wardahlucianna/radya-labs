using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.OnlineRegistration
{
    public class GetRegistrationDetailResult
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public string? RegistrationStartDate { get; set; }
        public string? RegistrationEndDate { get; set; }
        public NameValueVm Student { get; set; }
        public NameValueVm Homeroom { get; set; }
        public int Semester { get; set; }
        public int MinEffective { get; set; }
        public int MaxEffective { get; set; }
        public string? ReviewDate { get; set; }
    }
}
