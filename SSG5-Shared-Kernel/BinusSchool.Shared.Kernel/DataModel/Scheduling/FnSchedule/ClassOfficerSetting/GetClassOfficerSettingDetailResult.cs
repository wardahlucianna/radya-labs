using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassOfficerSetting
{
    public class GetClassOfficerSettingDetailResult : ItemValueVm
    {
        public CodeWithIdVm IdAcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm HomeRoom { get; set; }
        public int? Semester { get; set; }
        public UserHomeroomCaptain UserHomeroomCaptain { get; set; }
        public UserHomeroomViceCaptain UserHomeroomViceCaptain { get; set; }
        public UserHomeroomSecretary UserHomeroomSecretary { get; set; }
        public UserHomeroomTreasurer UserHomeroomTreasurer { get; set; }
    }

    public class UserHomeroomCaptain : NameValueVm
    {
        public bool? CaptainCanAssignClassDiary { get; set; }
    }

    public class UserHomeroomViceCaptain : NameValueVm
    {
        public bool? ViceCaptainCanAssignClassDiary { get; set; }
    }
    public class UserHomeroomSecretary : NameValueVm
    {
        public bool? SecretaryCanAssignClassDiary { get; set; }
    }
    public class UserHomeroomTreasurer : NameValueVm
    {
        public bool? TreasurerCanAssignClassDiary { get; set; }
    }
}
