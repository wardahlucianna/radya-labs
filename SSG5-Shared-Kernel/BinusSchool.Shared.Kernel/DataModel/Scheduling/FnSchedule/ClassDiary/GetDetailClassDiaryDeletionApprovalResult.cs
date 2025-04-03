using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary
{
    public class GetDetailClassDiaryDeletionApprovalResult
    {
        public string ClassDiaryId { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm Subject { get; set; }
        public int Semester { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public ItemValueVm ClassId { get; set; }
        public DateTime date { get; set; }
        public ItemValueVm ClassSettingType { get; set; }
        public CodeWithIdVm Level { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
        public string DeleteReason { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
        public bool DisabledButtonCalender { get; set; }
        public bool ShowButtonApproval { get; set; }
        public bool ShowButtonDecline { get; set; }
        public List<AttachmantClassDiary> Attachments { get; set; }


    }
}
