using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary
{
    public class AddClassDiaryDeletionApprovalRequest
    {
        public string ClassDiaryId { get; set; }
        public bool Approval { get; set; }
        public string Note { get; set; }
    }
}
