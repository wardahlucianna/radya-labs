using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule
{
    public class ScheduleDetailResult
    {
        public string IdSchedule { get; set; }
        public string IdLesson { get; set; }
        public CodeWithIdVm Day { get; set; }
        public CodeWithIdVm Session { get; set; }
        public string ClassID { get; set; }
        public string Subject { get; set; }
        public CodeWithIdVm Teacher { get; set; }
        public CodeWithIdVm Venue { get; set; }
        public DataModelGeneral Week { get; set; }
    }
}
