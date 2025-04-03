using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion
{
    public class GetMasterImmersionResult
    {
        public string IdImmersion { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public List<CodeWithIdVm> GradeList { get; set; }
        public string Description { get; set; }
        public string Destination { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public GetMasterImmersionResult_PIC PIC { get; set; }
        public int MinParticipant { get; set; }
        public int MaxParticipant { get; set; }
        public NameValueVm Currency { get; set; }
        public string CurrencySymbol { get; set; }
        public string CurrencyName { get; set; }
        public CodeWithIdVm ImmersionPaymentMethod { get; set; }
        public decimal RegistrationFee { get; set; }
        public decimal TotalCost { get; set; }
        public string PosterLink { get; set; }
        public string BrochureLink { get; set; }
    }

    public class GetMasterImmersionResult_PIC
    {
        public string IdBinusian { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
