using System.ComponentModel;

namespace BinusSchool.Workflow.Kernel.Enums
{
    public enum UserMessageType
    {
        Private = 1,
        Announcement,
        Feedback,
        
        [Description("ASC Timetable")]
        AscTimetable,

        [Description("Generate Schedule")]
        GenerateSchedule,
        Information,
    }
}
