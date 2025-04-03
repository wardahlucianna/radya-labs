using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiaryTypeSetting
{
    public class AddClassDiaryTypeSettingRequest
    {
        public string IdAcademicYear { get; set; }
        public string TypeName { get; set; }
        public int? OccurrencePerDay { get; set; }
        public int? MinimumStartDay { get; set; }
        public bool? AllowStudentEntryClassDiary { get; set; }
        public List<string> ExcludeClassIds { get; set; }
    }
}
