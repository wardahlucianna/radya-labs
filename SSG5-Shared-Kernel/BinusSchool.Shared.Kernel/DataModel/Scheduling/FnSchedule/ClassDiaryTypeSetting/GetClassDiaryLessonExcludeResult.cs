using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiaryTypeSetting
{
    public class GetClassDiaryLessonExcludeResult : ItemValueVm
    {
        public string Id { get; set; }
        public string IdLesson { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public string HomeRoom { get; set; }
        public CodeWithIdVm Subject { get; set; }
        public string ClassId { get; set; }
        public string Semester { get; set; }
    }
}
