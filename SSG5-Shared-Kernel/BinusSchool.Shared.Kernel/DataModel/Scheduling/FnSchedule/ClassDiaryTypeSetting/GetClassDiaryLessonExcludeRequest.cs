using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiaryTypeSetting
{
    public class GetClassDiaryLessonExcludeRequest : CollectionRequest
    {
        public string IdClassDiaryTypeSetting { get; set; }
        public string IdGrader { get; set; }
        public string IdSubject { get; set; }
        public string Semester { get; set; }
    }
}
