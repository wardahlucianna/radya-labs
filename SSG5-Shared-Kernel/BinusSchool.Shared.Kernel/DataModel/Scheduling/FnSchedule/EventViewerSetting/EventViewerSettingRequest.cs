using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.EventViewerSetting
{
    public class AddEventViewerSettingRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public List<EventViewerSettingDetailsRequest> EventViewerSettingDetails { get; set; }
    }

    public class EventViewerSettingDetailsRequest
    {
        public string? Id { get; set; }
        public ViewType ViewType { get; set; }
        public string IdRole { get; set; }
        public bool SpesificUser { get; set; }
        public string IdPosition { get; set; }
        public string IdUser { get; set; }
    }
}
