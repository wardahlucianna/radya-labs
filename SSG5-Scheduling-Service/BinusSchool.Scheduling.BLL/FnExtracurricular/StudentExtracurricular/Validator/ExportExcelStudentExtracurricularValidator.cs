using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.StudentExtracurricular;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.StudentExtracurricular.Validator
{
    public class ExportExcelStudentExtracurricularValidator : AbstractValidator<ExportExcelStudentExtracurricularRequest>
    {
        public ExportExcelStudentExtracurricularValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            //RuleFor(x => x.IdLevel).NotEmpty();
            //RuleFor(x => x.IdGrade).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
            //RuleFor(x => x.IdHomeroom).NotEmpty();
            RuleForEach(x => x.IdStudent).NotEmpty();
        }
    }
}
