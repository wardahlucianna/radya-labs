using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.ConsentForm
{
    public class GetPeriodEntryResult
    {
        public string IdSurveyPeriod { set; get; }
        public string IdGrade { set; get; }
        public string GradeName { set; get; }
        public DateTime StartDate { set; get; }
        public DateTime EndDate { set; get; }
        public bool HasRegularSchedule { set; get; }
     
        public SurveyPeriodRegularVm SurveyPeriodRegular { set; get; }

    }
    public class SurveyPeriodRegularVm
    {
        public DateTime StartDate { set; get; }
        public DateTime EndDate { set; get; }
        public bool RegularScheduleOpenStatus { set; get; }
       
    }
}
