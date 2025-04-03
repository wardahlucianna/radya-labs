using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom
{
    public class GetHomeroomDetailResult : DetailResult2
    {
        public CodeWithIdVm Acadyear { get; set; }
        public int Semester { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public string Pathway { get; set; }
        public IEnumerable<CodeWithIdVm> Pathways { get; set; }
        public CodeWithIdVm Venue { get; set; }
        public string Classroom { get; set; }
        public IEnumerable<HomeroomTeacherDetail> Teachers { get; set; }
    }

    public class HomeroomTeacherDetail
    {
        public CodeWithIdVm Teacher { get; set; }
        public CodeWithIdVm Position { get; set; }
        public bool HasAttendance { get; set; }
        public bool HasScore { get; set; }
        public bool ShowInReportCard { get; set; }
    }
}
