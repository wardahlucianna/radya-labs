using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiaryTypeSetting
{
    public class GetClassDiaryTypeSettingResult :  ItemValueVm
    {
        public CodeWithIdVm IdAcademicYear { get; set; }
        public string TypeName { get; set; }
        public string OccurencePerDayLimit { get; set; }
        public string MinimumStartDay { get; set; }
        public bool AllowStudentEntryClassDiary { get; set; }
        public bool CanModified { get; set; }
    }
}
