using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary
{
    public class CD4NotificationResult
    {
        public string Id { get; set; }
        public string AcademicYear { get; set; }
        public string Homeroom { get; set; }
        public string ClassId { get; set; }
        public string Date { get; set; }
        public string Type { get; set; }
        public string Topic { get; set; }
        public string RequestDate { get; set; }
        public string StatusApproval { get; set; }
        public List<string> IdRecipient { get; set; }
    }
}
