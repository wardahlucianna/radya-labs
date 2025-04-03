using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoachAttendance
{
    public class AddElectiveCoachAttendanceResult
    {
        public string Msg { set; get; }
        public string Date { set; get; }
        public List<string> Elective { set; get; }      
    }
}
