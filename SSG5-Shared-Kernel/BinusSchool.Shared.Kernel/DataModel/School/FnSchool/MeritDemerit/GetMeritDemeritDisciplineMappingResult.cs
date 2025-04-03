using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.MeritDemerit
{
    public class GetMeritDemeritDisciplineMappingResult : CodeWithIdVm
    {
        public string AcademicYear { get; set; }
        public string IdAcademicYear { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Category { get; set; }
        public string LevelInfraction { get; set; }
        public string IdLevelInfraction { get; set; }
        public string NameDiscipline { get; set; }
        public int? DisciplinePoint { get; set; }
        public bool IsDisableEdit { get; set; }
        public bool IsDisableDelete { get; set; }
    }
}
