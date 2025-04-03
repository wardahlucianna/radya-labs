using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetListParentPermissionResult
    {
        public string StudentId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string BinusianId { get; set; }
        public string IdGrade { get; set; }
        public string Grade { get; set; }
        public string IdHomeroom { get; set; }
        public string Homeroom { get; set; }
        public string ApprovalStatus { get; set; }
        public string Reason { get; set; }
    }
}
