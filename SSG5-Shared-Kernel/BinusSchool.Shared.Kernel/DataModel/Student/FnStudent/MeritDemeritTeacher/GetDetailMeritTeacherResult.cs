using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class GetDetailMeritTeacherResult
    {
        public string Student { get; set; }
        public string Homeroom { get; set; }
        public int TotalMerit { get; set; }
        public int TotalDemerit { get; set; }
        public string LevelOfInfraction { get; set; }
        public string Sanction { get; set; }
        public List<DetailMeritDemeritTeacher> Merit { get; set; }
    }

    public class DetailMeritDemeritTeacher
    {
        public string Id { get; set; }
        public DateTime? Date { get; set; }
        public string DeciplineName { get; set; }
        public string Point { get; set; }
        public string Note { get; set; }
        public string CreateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string Status { get; set; }
        public string LevelOfInfraction { get; set; }
        public bool IsDisabledDelete { get; set; }
        public bool IsDisabledEdit { get; set; }
    }
}
