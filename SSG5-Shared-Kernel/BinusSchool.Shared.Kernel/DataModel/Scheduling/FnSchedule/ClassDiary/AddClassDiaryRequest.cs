using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary
{
    public class AddClassDiaryRequest
    {
        public string AcademicYearId { get; set; }
        public string GradeId { get; set; }
        public string SubjectId { get; set; }
        public int Semester { get; set; }
        public List<string> LessonId { get; set; }
        public DateTime Date { get; set; }
        public string ClassDiaryTypeSettingId { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
        public List<AttachmantClassDiary> Attachments { get; set; }
    }

    public class AttachmantClassDiary
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string OriginalFilename { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
    }


}
