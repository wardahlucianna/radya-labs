using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentExitSetting
{
    public class GetAllStudentExitSettingRequest : CollectionRequest
    {
        public string AcademicYear { get; set; }
        public int? Semester { get; set; }
        public string? IdLevel { get; set; }
        public string? IdGrade { get; set; }
        public string? IdHomeroom { get; set; }
        public bool? IsExit { get; set; }

    }
}
