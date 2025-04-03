using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.MeritDemerit
{
    public class AddMeritDemeritDisciplineMappingCopyResult 
    {
        public string CountSucces { get; set; }
        public List<MeritDemeritDisciplineMapping> MeritDemeritDisciplineMappingFailed { get; set; }
    }

    public class MeritDemeritDisciplineMapping
    {
        public string AcademicYear { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Category { get; set; }
        public string LevelInfraction { get; set; }
        public string NameDiscipline { get; set; }
        public string Point { get; set; }
    }
}
