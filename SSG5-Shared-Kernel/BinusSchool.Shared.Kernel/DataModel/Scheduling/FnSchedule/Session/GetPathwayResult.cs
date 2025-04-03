using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Session
{
    public class GetPathwayResult: IItemValueVm
    {
        public string Id { get; set; }
        public string Acadyear { get; set; }
        public string Grade { get; set; }
        public string Pathway { get; set; }
        public string SchoolDay { get; set; }
        public int SessionId { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DurationInMinutes { get; set; }
        public string Description { get; set; }
        public string DayCode { get; set; }
    }
}
