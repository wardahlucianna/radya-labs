using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoachAttendance
{
    public class GetElectiveCoachAttendanceResult : ItemValueVm
    {
        public string IdUser { set; get; }
        public string IdExternalCoach { set; get; }
        public string UserName { set; get; }
        public string ElectivesName { set; get; }
        public string AttendanceDateTime { set; get; }        

    }   
}

