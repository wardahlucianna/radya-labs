using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant
{
    public class GetStudentParticipantByExtracurricularResult
    {
        public NameValueVm Student { get; set; }
        public NameValueVm AcademicYear { get; set; }
        public NameValueVm Homeroom { get; set; }
        public int Semester { get; set; }
        public DateTime JoinDate { get; set; }
        public decimal? Price { get; set; }
        public bool Status { get; set; }
        public bool? PaymentStatus { get; set; }
        public DateTime? DueDatePayment { get; set; }
        public bool EnableChange { get; set; }
        public bool EnableDelete { get; set; }
        public bool EnableResendEmail { get; set; }
        public bool EnableExtendDueDate { get; set; }
        public string IdTransanction { get; set; }
    }
}
