using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary
{
    public class DeleteClassDiaryRequest
    {
        public string ClassDiaryId { get; set; }
        public string DeleteReason { get; set; }
    }
}
