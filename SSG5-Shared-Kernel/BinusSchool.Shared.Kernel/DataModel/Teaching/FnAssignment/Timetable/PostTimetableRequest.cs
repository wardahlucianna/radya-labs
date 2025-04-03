using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.Timetable
{
    public class PostTimetableRequest
    {
        public List<PostTimeTableHeaderRequest> TimeTable { get; set; }
    }

    public class PostTimeTableHeaderRequest
    {
        public string Id { get; set; }
        public bool Status { get; set; }
        public List<PostTimeTableDetailRequest> TimeTableDetail { get; set; }
    }

    public class PostTimeTableDetailRequest
    {
        public string Id { get; set; }
        public string IdSchoolDivision { get; set; }
        public int Count { get; set; }
        public int Length { get; set; }
        public string Week { get; set; }
        public string IdSchoolTerm { get; set; }
        public string IdVenue { get; set; }
        public string IdUserTeaching { get; set; }
    }
}
