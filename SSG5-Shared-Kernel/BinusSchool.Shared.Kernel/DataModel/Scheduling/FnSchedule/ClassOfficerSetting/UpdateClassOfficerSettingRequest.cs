using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassOfficerSetting
{
    public class UpdateClassOfficerSettingRequest
    {
        public string IdHomeRoom { get; set; }
        public string IdUserHomeroomCaptain { get; set; }
        public bool? CapatainCanAssignClassDiary { get; set; }
        public string IdUserHomeroomViceCaptain { get; set; }
        public bool? ViceCapatainCanAssignClassDiary { get; set; }
        public string IdUserHomeroomSecretary { get; set; }
        public bool? SecretaryCanAssignClassDiary { get; set; }
        public string IdUserHomeroomTreasurer { get; set; }
        public bool? TreasurerCanAssignClassDiary { get; set; }
    }
}
