using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.EventViewerSetting
{
    public class GetEventViewerSettingRequest : CollectionSchoolRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
    }
}
