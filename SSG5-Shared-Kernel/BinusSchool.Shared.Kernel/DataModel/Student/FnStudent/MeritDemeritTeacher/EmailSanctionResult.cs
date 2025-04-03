using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class EmailSanctionResult
    {
        public string Id { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string MeritTotal { get; set; }
        public string DemeritTotal { get; set; }
        public string LevelOfInfraction { get; set; }
        public string Sanction { get; set; }
        public string LastUpdate { get; set; }
        public string IdUser { get; set; }
        public string SchoolName { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdAcadYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string Link { get; set; }
        public bool IsPoint { get; set; }
    }
}
