using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiaryTypeSetting
{
    public class CopySettingClassDiaryTypeSettingRequest
    {
        public string IdAcademicYearCopyTo { get; set; }
        public string IdAcademicYearCopyFrom { get; set; }
        public List<string> IdClassDiaryTypeSettings { get; set; }
    }
}
