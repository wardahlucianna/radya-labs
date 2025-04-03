using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetStudentParticipantResult : CodeWithIdVm
    {
        public string Fullname { get; set; }
        public string BinusianID { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public string Award { get; set; }

    }
}