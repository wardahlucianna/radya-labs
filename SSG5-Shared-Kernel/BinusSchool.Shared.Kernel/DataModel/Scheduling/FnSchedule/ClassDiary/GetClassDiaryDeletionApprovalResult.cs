using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary
{
    public class GetClassDiaryDeletionApprovalResult : CodeWithIdVm
    {
        public string AcademicYear { get; set; }
        public string Grade { get; set; }
        public string Subject { get; set; }
        public string Semester { get; set; }
        public string Homeroom { get; set; }
        public string ClassId { get; set; }
        public DateTime ClassDiaryDate { get; set; }
        public string ClassDiaryTypeSetting { get; set; }
        public string ClassDiaryTopic { get; set; }
        public DateTime? RequestDate { get; set; }
        public string Status { get; set; }
        public bool ShowButtonApproval { get; set; }
    }
}
