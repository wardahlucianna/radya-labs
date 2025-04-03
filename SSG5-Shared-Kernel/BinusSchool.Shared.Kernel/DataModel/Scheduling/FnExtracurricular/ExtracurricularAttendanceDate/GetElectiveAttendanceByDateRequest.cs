using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendanceDate
{
    public class GetElectiveAttendanceByDateRequest
    {       
        public string IdUser { set; get; }
        public DateTime Date { set; get; }
        public string IdSchool { get; set; }
    }
}
