using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Grade
{
    public class GradeCodeAcademicYearsResult : CodeWithIdVm
    {
        public IEnumerable<CodeWithIdVm> Grades { get; set; }
    }
}
