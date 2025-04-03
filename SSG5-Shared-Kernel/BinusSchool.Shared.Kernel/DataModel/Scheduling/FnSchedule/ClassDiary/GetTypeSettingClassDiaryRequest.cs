using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary
{
    public class GetTypeSettingClassDiaryRequest
    {
        public string AcademicYearId { get; set; }
        public string GradeId { get; set; }
        public string SubjectId { get; set; }
        public int Semester { get; set; }
        public List<string> LessoinId { get; set; }
        public bool IsStudent { get; set; }
    }
}
