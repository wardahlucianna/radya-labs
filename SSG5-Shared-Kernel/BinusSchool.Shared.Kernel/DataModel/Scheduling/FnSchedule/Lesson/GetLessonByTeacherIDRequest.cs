using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson
{
    public class GetLessonByTeacherIDRequest : CollectionRequest
    {
        public string IdTeacher { get; set; }
    }
}
