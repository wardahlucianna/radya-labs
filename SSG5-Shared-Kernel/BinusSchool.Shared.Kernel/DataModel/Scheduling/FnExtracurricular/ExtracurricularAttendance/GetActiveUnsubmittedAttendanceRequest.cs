using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class GetActiveUnsubmittedAttendanceRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        //public string IdAcademicYear { get; set; }
        //public int Semeter { get; set; }
        public string IdUser { get; set; }
    }
}
