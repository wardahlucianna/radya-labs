using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary
{
    public class CheckClassLimitClassDiaryResult
    {
        public string Homeroom { get; set; }
        public string ClassId { get; set; }
        public int LimitPerDay { get; set; }

    }

    public class ListStudentByClassDiary
    {
        public string IdHomeroom { get; set; }
        public string IdLesson { get; set; }
        public string IdStudent { get; set; }
        //public List<string> IdStudent { get; set; }

    }
}
