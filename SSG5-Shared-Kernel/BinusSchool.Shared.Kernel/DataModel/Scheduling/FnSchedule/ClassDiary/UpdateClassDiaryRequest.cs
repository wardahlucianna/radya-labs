using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary
{
    public class UpdateClassDiaryRequest
    {
        public string ClassDiaryId { get; set; }
        public DateTime Date { get; set; }
        public string ClassDiaryTypeSettingId { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
        public List<AttachmantClassDiary> Attachments { get; set; }
    }
}
