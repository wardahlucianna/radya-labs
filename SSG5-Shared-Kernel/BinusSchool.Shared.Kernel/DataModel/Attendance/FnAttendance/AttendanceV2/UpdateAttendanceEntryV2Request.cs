using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class UpdateAttendanceEntryV2Request
    {
        ///// <summary>
        ///// IdHomeroom
        ///// </summary>
        //public string IdHomeroom { get; set; }
        //public string IdSchool { get; set; }
        //public DateTime Date { get; set; }
        ///// <summary>
        ///// Fill this field from <see cref="BinusSchool.Common.Constants.PositionConstant"/>
        ///// </summary>
        public string CurrentPosition { get; set; }
        //public string ClassId { get; set; }
        //public string IdSession { get; set; }
        ///// <summary>
        ///// Only work when absent term is Session
        ///// </summary>
        public bool CopyToNextSession { get; set; }
        //public bool SendLateEmailToParent { get; set; }
        //public bool SendAbsentEmailToParent { get; set; }
        public IEnumerable<UpdateAttendanceEntryStudent> Entries { get; set; }
        public string IdUser { get; set; }
    }

    public class UpdateAttendanceEntryStudent
    {
        public string IdScheduleLesson { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdAttendanceMapAttendance { get; set; }
        public IEnumerable<string> IdWorkhabits { get; set; }
        public string LateInMinute { get; set; }
        public string File { get; set; }
        public string Note { get; set; }
    }
}
