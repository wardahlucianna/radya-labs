using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.OnlineRegistration
{
    public class GetExtracurricularListByStudentResult : ItemValueVm
    {
        public NameValueVm Extracurricular { get; set; }
        public string ExtracurricularDescription { get; set; }
        public decimal ExtracurricularPrice { get; set; }
        public List<DayTimeSchedule> ScheduleDayTimeList { get; set; }
        public int AvailableSeat { get; set; }
        //public int? SelectedPriority { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsSelected { get; set; }
        public bool HasCreatedInvoice { get; set; }
        public decimal Price { get; set; }
        public GetExtracurricularListByStudentResult_PaymentStatus PaymentStatus { get; set; }
    }
    public class DayTimeSchedule
    {
        public string IdExtracurricular { get; set; }
        public string IdDay { get; set; }
        public string Day { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    public class GetExtracurricularListByStudentResult_PaymentStatus
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
