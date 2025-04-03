using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.DataByPosition
{
    public class LevelByPositionResult
    {
        public string IdUser { get; set; }
        public string Posistion {  get; set; }
        public ItemValueVm Level {  get; set; }
    }
}
