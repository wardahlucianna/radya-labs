using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary
{
    public class GetLevelClassDiaryResult : ItemValueVm
    {
        public int OrderNumber { get; set; }
        public List<GetGradeClassDiary> Grade { get; set; }
    }

    
    public class GetGradeClassDiary : ItemValueVm
    {
        public List<GetSubjectClassDiary> Subject {  get; set; }
    }

    public class GetSubjectClassDiary : ItemValueVm
    {
        public List<GetHomeroomClassDiary> Homeroom { get; set; }
        public List<string> ClassId { get; set; }
    }

    public class GetHomeroomClassDiary : ItemValueVm
    {
        public List<string> ClassId { get; set; }
    }
}
