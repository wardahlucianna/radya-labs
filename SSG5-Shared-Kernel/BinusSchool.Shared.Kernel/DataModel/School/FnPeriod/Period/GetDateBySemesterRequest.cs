using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnPeriod.Period
{
    public class GetDateBySemesterRequest
    {
        public string IdGrade { get; set; }
        public int Semester { get; set; } = 1;
    }
}
