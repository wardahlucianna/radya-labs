using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MapStudentGrade;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MapStudentGrade.Validator
{
    public class CopyNextAYMapStudentGradeValidator : AbstractValidator<CopyNextAYMapStudentGradeRequest>
    {
        public CopyNextAYMapStudentGradeValidator()
        {
            //RuleFor(x => x.IdAcademicYearTarget).NotNull();
            //RuleFor(x => x.IdAcademicYearSource).NotNull();
            //RuleFor(x => x.IdLevel).NotNull();
            //RuleFor(x => x.IdGrade).NotNull();
        }
    }
}
