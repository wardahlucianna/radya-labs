using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.ClassSession
{
    public class GetClassSessionResult : ItemValueVm
    {
        public string ClassId { get; set; }
        public IEnumerable<SessionOfClass> Sessions { get; set; }
    }

    public class SessionOfClass : IUniqueId
    {
        public string Id { get; set; }
        public string SessionId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}