using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class GetStudentInformationByAcademicYearRequest
    {
        public string IdStudent { get; set; }
        public List<string> IdAcademicYears { get; set; }
    }
}
