using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.MapStudentHomeroom
{
    public class AddMapStudentHomeroomRequest
    {
        public string IdHomeroom { get; set; }
        public IEnumerable<MapStudentToHomeroom> Students { get; set; }
        public bool FromCopySemester { get; set; } = false;
    }

    public class MapStudentToHomeroom
    {
        public string IdStudent { get; set; }
        public Gender Gender { get; set; }
        public string Religion { get; set; }
    }
}
