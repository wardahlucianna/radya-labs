using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetCalendarScheduleResult : ItemValueVm
    {
        public string Name { get; set; }
        public string CreateBy { get; set; }
        public string ClassId { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public ItemValueVm Department { get; set; }
        public NameValueVm Teacher { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public ItemValueVm Venue { get; set; }
        public CalendarEventTypeVm EventType { get; set; }
        public List<AttachmantClassDiary> Attachment { get; set; }
        public List<StudentBooking> StudentBooking { get; set; }
        public bool IsChange { get; set; }
        public bool IsCancel { get; set; }
    }
}
