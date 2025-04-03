using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoachAttendance
{
    public class GetElectiveCoachAttendanceRequest : CollectionRequest
    {
        public string IdAcademicYear {set;get;}
        public int Semester { set; get; }
        public string IdUser { set; get; }
    }
}
