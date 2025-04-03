using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetWeekByGradeSubjectResult
    {
        public List<Subject> Subjects { get; set; }
        public string IdGrade { get; set; }
    }

    public class Subject : CodeWithIdVm
    {
        public WeekVariantSubject WeekVariant { get; set; }
    }

    public class WeekVariantSubject : CodeWithIdVm
    {
        public List<Week> Weeks { get; set; }  
    }

    public class Week  : CodeWithIdVm
    {

    }
}
