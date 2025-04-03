using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.Timetable
{
    public class AddTimeTableRequest
    {
        public string Id { get; set; }
        public bool CanDeleted { get; set; }
        public bool IsMerge { get; set; }
        public bool Status { get; set; }
        public IEnumerable<TimeTableDetailRequest> TimeTableDetailRequests { get; set; }
    }

    //public class TimeTableHeaderRequest
    //{
    //    public string Id { get; set; }
    //    public bool Status { get; set; }
    //    public List<TimeTableDetailRequest> TimeTableDetail { get; set; }
    //}

    public class TimeTableDetailRequest
    {
        public string Id { get; set; }
        public string IdTimeTablePrefHeader { get; set; }

        public int Count { get; set; }
        public int Length { get; set; }
    }
}
