using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiaryTypeSetting
{
    public class CopySettingClassDiaryTypeSettingResult
    {
        public string CountSucces { get; set; }
        public List<CopySettingTypeSetting> CopySettingTypeSettings { get; set; }
    }

    public class CopySettingTypeSetting
    {
        public CodeWithIdVm IdAcademicYear { get; set; }
        public string TypeName { get; set; }
        public int OccurencePerDay { get; set; }
        public int MinimumStartDay { get; set; }
        public bool AllowStudentEntryClassDiary { get; set; }
    }
}
