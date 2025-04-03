using System;
using System.Collections.Generic;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrAttendanceSummaryLog : AuditEntity, IAttendanceEntity
    {
        public TrAttendanceSummaryLog()
        {
            Id = Guid.NewGuid().ToString();
        }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsDone { get; set; }
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public virtual ICollection<TrAttdSummaryLogSch> AttdSummaryLogSch { get; set; }
    }

    public class TrAttendanceSummaryLogConfiguration : AuditEntityConfiguration<TrAttendanceSummaryLog>
    {
    }
}
