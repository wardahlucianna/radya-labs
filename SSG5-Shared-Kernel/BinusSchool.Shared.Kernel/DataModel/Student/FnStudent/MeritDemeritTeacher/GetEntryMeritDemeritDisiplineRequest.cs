using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class GetEntryMeritDemeritDisiplineRequest
    {
        public string IdAcademicYear { get; set; }
        public string Idlevel { get; set; }
        public string IdGrade { get; set; }
        public MeritDemeritCategory Category { get; set; }
        public string IdLevelInfraction { get; set; }
    }
}
