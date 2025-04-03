using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion
{
    public class GetMasterImmersionDetailResult
    {
        public string IdImmersion { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public List<CodeWithIdVm> LevelList { get; set; }
        public List<CodeWithIdVm> GradeList { get; set; }
        public ItemValueVm Semester { get; set; }
        public string Destination { get; set; }
        public ItemValueVm ImmersionPeriod { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public NameValueVm BinusianPIC { get; set; }
        public string PICEmail { get; set; }
        public string PICPhone { get; set; }
        public int MinParticipant { get; set; }
        public int MaxParticipant { get; set; }
        public ItemValueVm Currency { get; set; }
        public CodeWithIdVm ImmersionPaymentMethod { get; set; }
        public decimal RegistrationFee { get; set; }
        public decimal TotalCost { get; set; }
        public string PosterLink { get; set; }
        public string BrochureLink { get; set; }
    }
}
