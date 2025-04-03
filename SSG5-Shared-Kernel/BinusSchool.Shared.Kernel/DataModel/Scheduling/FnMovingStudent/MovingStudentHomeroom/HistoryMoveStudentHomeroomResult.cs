using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MovingStudentHomeroom
{
    public class HistoryMoveStudentHomeroomResult : CodeWithIdVm
    {
        public string HomeroomNew { get; set; }
        public string HomeroomOld { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string Note { get; set; }
    }
}
