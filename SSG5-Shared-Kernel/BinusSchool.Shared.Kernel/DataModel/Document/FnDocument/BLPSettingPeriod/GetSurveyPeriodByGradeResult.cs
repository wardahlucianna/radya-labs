using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.BLPSettingPeriod
{
    public class GetSurveyPeriodByGradeResult
    {
        public string IdSurveyPeriod { get; set; }     
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool AllowedToEdit { get; set; }
        public bool CustomSchedule { get; set; }
        public GetSurveyPeriodByGradeResult_CustomScheduleVm CustomConsentSchedule { get; set; }
        public List<GetSurveyPeriodByGradeResult_ClearanceWeekVm> CustomClearanceSchedule { get; set; }
    }

    public class GetSurveyPeriodByGradeResult_CustomScheduleVm
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool RegularScheduleOpenStatus { get; set; }

    }
    public class GetSurveyPeriodByGradeResult_ClearanceWeekVm
    {
        public int WeekID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdBLPGroup { get; set; }
    }
}
