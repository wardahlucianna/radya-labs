using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.BLPSettingPeriod
{
    public class GetBLPSettingDetailResult
    {
        public string IdSurvey { get; set; }
        public string IdSurveyCategory { get; set; }
        public string SurveyCategoryName { get; set; }
        public string IdAcademicYear { get; set; }
        public string AcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdLevel { get; set; }
        public string LevelName { get; set; }
        public string IdGrade { get; set; }
        public string Grade { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool HasConsentCustomSchedule { get; set; }
        public GetBLPSettingDetailResult_CustomScheduleVm ConsentCustomSchedule { get; set; }
        public bool HasClearanceCustomSchedule { get; set; }        
        public List<GetBLPSettingDetailResult_ClearanceWeekVm> ClearanceWeekPeriod { get; set; }
    }
    public class GetBLPSettingDetailResult_CustomScheduleVm
    {
        public string IdConsentCustomSchedule { get; set; }
        public string StartDay { get; set; }
        public string EndDay { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
    public class GetBLPSettingDetailResult_ClearanceWeekVm
    {
        
        public string IdClearanceWeekPeriod { get; set; }
        public int WeekID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdBLPGroup { get; set; }
    }
}
