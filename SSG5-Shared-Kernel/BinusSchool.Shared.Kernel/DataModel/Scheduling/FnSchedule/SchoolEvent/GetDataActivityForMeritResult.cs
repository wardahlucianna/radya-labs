using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetDataActivityForMeritResult : CodeWithIdVm
    {
        public string IdEvent { get; set;}
        public string Fullname { get; set;}
        public string BinusianID { get; set;}
        public string IdLevel { get; set;}
        public string Level { get; set;}
        public string IdGrade { get; set;}
        public string Grade { get; set;}
        public string IdHomeroom { get; set;}
        public string IdClassroom { get; set;}
        public string Classroom { get; set;}
        public string IdHomeroomStudent { get; set;}
        public int Semester { get; set;}
        public string IdActivity { get; set;}
        public string Activity { get; set;}
        public string IdAward { get; set;}
        public string Award { get; set;}
    }
}