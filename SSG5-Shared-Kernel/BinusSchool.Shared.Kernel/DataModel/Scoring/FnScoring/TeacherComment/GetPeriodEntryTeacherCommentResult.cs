using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherComment
{
    public class GetPeriodEntryTeacherCommentResult
    {
        public bool IsPeriodSetTeacherComment { get; set; }
        public string IdCommentSetting { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsFinalApprover { get; set; }
        public bool? IsEditable { get; set; }
        public bool? IsLocked { get; set; }
    }
}
