﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary
{
    public class GetCheackingTypeSettingRequest
    {
        public string AcademicYearId { get; set; }
        public string GradeId { get; set; }
        public string SubjectId { get; set; }
        public int Semester { get; set; }
        public List<string> HomeroomId { get; set; }
        public List<string> LessoinId { get; set; }
        public DateTime Date { get; set; }
        public string TypeSettingId { get; set; }
    }
}
