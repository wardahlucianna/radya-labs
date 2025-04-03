using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.BLPSettingPeriod
{
    public class UpdateBLPSettingPeriodRequest
    {
        public string IdSurveyPeriod { get; set; }
        public string IdSurveyCategory { get; set; }
        //public string IdAcademicYear { get; set; }
        //public int Semester { get; set; }
        public string IdGrade { get; set; }
        public DateTime StartDateSurvey { get; set; }
        public DateTime EndDateSurvey { get; set; }
        public bool HasConsentCustomSchedule { get; set; }
        public UpdateBLPSettingPeriod_CustomScheduleVm ConsentCustomSchedule { get; set; }
        public bool HasClearanceWeekPeriod { get; set; }
        public List<UpdateBLPSettingPeriod_ClearanceWeekVm> ClearanceWeekPeriod { get; set; }
    }

    public class UpdateBLPSettingPeriod_CustomScheduleVm
    {
        public string StartDay { get; set; }
        public string EndDay { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
    public class UpdateBLPSettingPeriod_ClearanceWeekVm
    {
        public int WeekID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdBLPGroup { get; set; }
    }
}
