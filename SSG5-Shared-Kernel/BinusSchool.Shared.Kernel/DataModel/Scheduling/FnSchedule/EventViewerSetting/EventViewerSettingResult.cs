using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.EventViewerSetting
{
    public class EventViewerSettingResult : CodeWithIdVm
    {
        public CodeWithIdVm School { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public ViewType ViewType { get; set; }
        public CodeWithIdVm Role { get; set; }
        public CodeWithIdVm Position { get; set; }
        public CodeWithIdVm User { get; set; }
        public bool SpesificUser { get; set; }
        
    }
}
