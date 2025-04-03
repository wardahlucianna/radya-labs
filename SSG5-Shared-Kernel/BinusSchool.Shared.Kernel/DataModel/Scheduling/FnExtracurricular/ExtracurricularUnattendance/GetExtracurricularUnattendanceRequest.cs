using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularUnattendance
{
    public class GetExtracurricularUnattendanceRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
    }
}
