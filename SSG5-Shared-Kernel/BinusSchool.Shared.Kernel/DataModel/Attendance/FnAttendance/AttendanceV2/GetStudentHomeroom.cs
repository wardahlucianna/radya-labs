using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class GetStudentHomeroom
    {
        public ItemValueVm Homeroom {  get; set; }
        public string IdStudent {  get; set; }
        public string IdGrade {  get; set; }
        public int Semester {  get; set; }
        public DateTime EffectiveDate {  get; set; }
        public DateTime? DateIn {  get; set; }
        public bool IsFromMaster {  get; set; }
        public bool IsShowHistory {  get; set; }
    }
}
