using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.ClearanceForm
{
    public class GetClearanceFormPeriodResult
    {
        public bool InPeriod { get; set; }
        public bool IsOpened { get; set; }
        public bool IsReguler { get; set; }
        public bool IsThisPeriodAnySubmitted { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<ClearanceFormPeriod_Student> Student { get; set; }
    }

    public class ClearanceFormPeriod_Student
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string IdSurveyPeriod { get; set; }
        public string IdClearanceWeekPeriod { get; set; }
    }
}
