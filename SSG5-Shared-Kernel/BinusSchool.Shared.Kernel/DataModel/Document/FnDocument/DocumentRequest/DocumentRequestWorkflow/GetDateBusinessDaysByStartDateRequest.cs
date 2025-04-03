using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow
{
    public class GetDateBusinessDaysByStartDateRequest
    {
        public string IdSchool { get; set; }
        public DateTime StartDate { get; set; }
        public int TotalDays { get; set; }
        public bool CountHoliday { get; set; }
    }

    public class GetDateBusinessDaysByStartDateRequest_DateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
    }
}
