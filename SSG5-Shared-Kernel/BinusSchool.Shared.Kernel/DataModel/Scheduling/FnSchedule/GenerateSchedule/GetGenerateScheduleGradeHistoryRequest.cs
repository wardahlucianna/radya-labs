using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetGenerateScheduleGradeHistoryRequest : CollectionRequest
    {
        public string IdGrade {get;set;}
        public string ClassId { get; set; }
        public string IdAscTimetable { get; set; }
        public DateTime StartPeriod {get;set;}
        public DateTime EndPeriod {get;set;}
    }
}
