using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class Gc2Result
    {
        public List<Gc1Result> DataOld { get; set; }
        public List<Gc1Result> DataNew { get; set; }
    }
}
