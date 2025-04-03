using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiaryTypeSetting
{
    public class GetClassDiaryTypeSettingDetailResult : ItemValueVm
    {
        public CodeWithIdVm IdAcademicYear { get; set; }
        public string TypeName { get; set; }
        public int OccurencePerDayLimit { get; set; }
        public int MinimumStartDay { get; set; }
        public bool AllowStudentEntryClassDiary { get; set; }
        public List<ExcludeClassId> ExcludeClassIds { get; set; }
    }

    public class ExcludeClassId
    {
        public string Id { get; set; }
        public string IdLesson { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public string HomeRoom { get; set; }
        public CodeWithIdVm Subject { get; set; }
        public string ClassId { get; set; }
    }
}
