using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BinusSchool.Common.Model.Enums
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
