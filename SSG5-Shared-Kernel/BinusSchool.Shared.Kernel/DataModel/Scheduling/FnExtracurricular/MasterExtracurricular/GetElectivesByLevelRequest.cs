using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class GetElectivesByLevelRequest
    {
        public string IdUser { get; set; }
        public string IdAcademicYear { get; set; }
        public string? IdLevel { get; set; }

        public int Semester { get; set; }
    }
}
