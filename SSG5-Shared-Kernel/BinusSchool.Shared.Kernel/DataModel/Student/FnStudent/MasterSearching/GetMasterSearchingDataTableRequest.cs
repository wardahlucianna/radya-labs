using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MasterSearching
{
    public class GetMasterSearchingDataTableRequest
    {
        public string IdSchool { get; set; }
        public string SchoolName { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string? IdGrade { get; set; }
        public string? IdHomeroom { get; set; }
        public string? IdLevel { get; set; }
        public int? IdStudentStatus { get; set; }
        public List<string> FieldData { get; set; }
        public string SearchByFieldData { get; set; }
        public string Keyword { get; set; }
    }
}
