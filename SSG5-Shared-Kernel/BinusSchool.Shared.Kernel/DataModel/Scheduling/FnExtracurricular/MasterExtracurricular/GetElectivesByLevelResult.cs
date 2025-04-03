using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class GetElectivesByLevelResult
    {
        public string Id { get; set; }
        public string ElectiveName { get; set; }
        public string Grades { get; set; }

        public int Semester { get; set; }
    }
}
