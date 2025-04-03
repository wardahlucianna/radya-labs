using System;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry
{
    public class UpdateAllAttendanceEntryRequest
    {
        /// <summary>
        /// IdHomeroom
        /// </summary>
        public string Id { get; set; }
        public string ClassId { get; set; }
        public DateTime Date { get; set; }
        /// <summary>
        /// Fill this field from <see cref="BinusSchool.Common.Constants.PositionConstant"/>
        /// </summary>
        public string CurrentPosition { get; set; }
        public string IdSession { get; set; }
    }
}
