using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.NotificationHistory
{
    public class GetNotificationHistoryResult : ItemValueVm
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Feature { get; set; }
        public string Scenario { get; set; }
        public IDictionary<string, string> Data { get; set; }
        public DateTime? ReadDate { get; set; }
        public bool IsRead => ReadDate.HasValue;
        public AuditableResult Audit { get; set; }
        public bool IsDelete { get; set; }
    }
}
