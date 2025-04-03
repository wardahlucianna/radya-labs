using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentPoint
{
    public class GetDetailStudentMeritDemeritPointResult
    {
        public CodeWithIdVm Acadyear { get; set; }
        public int MeritPoint { get; set; }
        public int DemeritPoint { get; set; }
        public string LevelOfInteraction { get; set; }
        public string Sanction { get; set; }
        public IEnumerable<PointDetailResult> DetailMeritPoint { get; set; }
        public IEnumerable<PointDetailResult> DetailDemeritPoint { get; set; }

    }

    public class PointDetailResult
    {
        public DateTime Date { get; set; }
        public string DiciplineName { get; set; }
        public string Note { get; set; }
        public int Point { get; set; }
        public string TeacherName { get; set; }
    }
}
