using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.MeritDemerit
{
    public class AddMeritDemeritDisciplineMappingRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public MeritDemeritCategory Category { get; set; }
        public string IdLevelInfraction { get; set; }
        public string DisciplineName { get; set; }
        public string Point { get; set; }
    }
}
