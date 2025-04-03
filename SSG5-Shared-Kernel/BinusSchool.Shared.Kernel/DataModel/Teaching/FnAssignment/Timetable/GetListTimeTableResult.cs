using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.Timetable
{
    public class GetListTimeTableResult : CodeWithIdVm
    {
        public int No { get; set; }
        public bool IsMarge { get; set; }
        public SubjectVm Subject { get; set; }
        public int TotalSession { get; set; }
        public CodeView Class { get; set; }
        public CodeView AcadYear { get; set; }
        public CodeView Level { get; set; }
        public CodeView Grade { get; set; }
        public bool Status { get; set; }
        public List<TimeTableDetailVm> TimeTableDetail { get; set; }
        public List<string> IdChilds { get; set; }
        public CodeView Department { get; set; }
        public CodeView Streaming { get; set; }
        public bool IsParent { get; set; }
        public string ClassroomDivision { get; set; }
    }

    public class TimeTableDetailVm
    {
        public string Id { get; set; }
        public int TotalLoad { get; set; }
        public TeacherVm Teacher { get; set; }
        public BuildingVenueVm BuildingVanue { get; set; }
        public int Count { get; set; }
        public int Lenght { get; set; }
        public CodeView Division { get; set; }
        public CodeView Term { get; set; }
        public string Week { get; set; }
        public string Department { get; set; }
        public string Level { get; set; }
        public string Streaming { get; set; }

    }

    public class SubjectVm : ItemValueVm
    {
        public string SubjectName { get; set; }
        public string SubjectId { get; set; }
    }

    public class BuildingVenueVm : CodeView
    {
        public string BuildingCode { get; set; }
        public string BuildingDesc { get; set; }
    }

    public class CodeView : CodeVm
    {
        public string Id { get; set; }
        public string IdMapping { get; set; }
    }

    public class TeacherVm : CodeWithIdVm
    {
        public string BinusianId { get; set; }
        public int? TotalLoad { get; set; }
    }
}
