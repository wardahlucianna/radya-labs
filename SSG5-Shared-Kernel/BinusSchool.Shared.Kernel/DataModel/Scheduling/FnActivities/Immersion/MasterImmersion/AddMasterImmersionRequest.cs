using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion
{
    public class AddMasterImmersionRequest
    {
        public List<string> IdGradeList { get; set; }
        public int Semester { get; set; }
        public string Destination { get; set; }
        public string IdImmersionPeriod { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdBinusianPIC { get; set; }
        public string PICEmail { get; set; }
        public string PICPhone { get; set; }
        public int MinParticipant { get; set; }
        public int MaxParticipant { get; set; }
        public string IdCurrency { get; set; }
        public string IdImmersionPaymentMethod { get; set; }
        public decimal RegistrationFee { get; set; }
        public decimal TotalCost { get; set; }
    }
}
