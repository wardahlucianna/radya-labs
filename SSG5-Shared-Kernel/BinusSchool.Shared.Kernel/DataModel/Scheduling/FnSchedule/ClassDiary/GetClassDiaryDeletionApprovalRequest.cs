﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary
{
    public class GetClassDiaryDeletionApprovalRequest : CollectionSchoolRequest
    {
        public string AcademicYearId { get; set; }
        public string IdLevel { get; set; }
        public string GradeId { get; set; }
        public string SubjectId { get; set; }
        public int? Semester { get; set; }
        public string HomeroomId { get; set; }
        public string LessonId { get; set; }
        public DateTime? ClassDiaryDate { get; set; }
        public string ClassDiaryTypeSettingId { get; set; }
        public string ClassDiaryStatus { get; set; }
        public DateTime? RequestDate { get; set; }
        public string RequestBy { get; set; }
        public string UserId { get; set; }
    }
}
