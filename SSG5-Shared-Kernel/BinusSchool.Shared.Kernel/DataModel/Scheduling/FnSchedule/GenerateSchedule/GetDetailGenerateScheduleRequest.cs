using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetDetailGenerateScheduleRequest
    {
        public string IdGrade { get; set; }
        public string IdAcademicYears { get; set; }
        public string IdAscTimetable { get; set; }
        public string IdClass { get; set; }
        public string IdSubject { get; set; }
        /// <summary>
        /// kalo mau ambil data get by mount ini harus di isi 
        /// </summary>
        public DateTime? DateMountYers { get; set; }
        public DateTime? ScheduleDate { get; set; }
    }
}
