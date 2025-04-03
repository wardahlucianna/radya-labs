using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.GcCorner
{
    public class GetGcCornerYourCounselorRequest
    {
        public string IdStudent { get; set; }

        public string IdAcademicYear { get; set; }
    }
}
