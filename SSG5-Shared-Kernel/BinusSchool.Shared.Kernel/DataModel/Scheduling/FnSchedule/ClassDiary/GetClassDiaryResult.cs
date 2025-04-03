using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary
{
    public class GetClassDiaryResult : CodeWithIdVm
    {
        public string AcademicYear { get; set; }
        public string Grade { get; set; }
        public string Subject { get; set; }
        public int Semester { get; set; }
        public string Homeroom { get; set; }
        public string ClassId { get; set; }
        public DateTime ClassDiaryDate { get; set; }
        public string ClassDiaryTypeSetting { get; set; }
        public string ClassDiaryTopic { get; set; }
        public string Status { get; set; }
        public ItemValueVm Teacher { get; set; }

        public bool ShowButtonEdit { get; set; }
        public bool ShowButtonDelete  { get; set; }
    }
}
