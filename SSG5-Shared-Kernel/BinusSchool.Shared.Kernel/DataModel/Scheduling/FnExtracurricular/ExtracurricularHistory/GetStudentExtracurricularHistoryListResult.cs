using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularHistory
{
    public class GetStudentExtracurricularHistoryListResult
    {
        public ItemValueVm AcademicYear { get; set; }
        public int Semester { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public ItemValueVm Student { get; set; }
        public List<ItemValueVm> Extracurricular { get; set; }
    }
}
