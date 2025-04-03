namespace BinusSchool.Util.Kernel.Enums
{
    public enum AttendanceEntryStatus
    {
        /// <summary>Pending attendance entry</summary>
        Pending,
        /// <summary>Submitted attendance entry</summary>
        Submitted,
        /// <summary>Dont insert into TrAttendanceEntry (Unsubmitted equal to no data/row not created)</summary>
        Unsubmitted
    }
}
