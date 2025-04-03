using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassOfficerSetting
{
    public class DownloadClassOfficerSettingResult : ItemValueVm
    {
        public CodeWithIdVm IdAcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm HomeRoom { get; set; }
        public int? Semester { get; set; }
        public UserHomeroomCaptain Captain { get; set; }
        public UserHomeroomViceCaptain ViceCaptain { get; set; }
        public UserHomeroomSecretary Secretary { get; set; }
        public UserHomeroomTreasurer Treasurer { get; set; }
        public DateTime? LastModified { get; set; }
        public string UserModified { get; set; }
    }
}
