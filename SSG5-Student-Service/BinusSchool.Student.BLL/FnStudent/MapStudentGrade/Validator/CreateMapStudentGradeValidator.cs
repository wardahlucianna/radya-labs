using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MapStudentGrade;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MapStudentGrade.Validator
{
    public class CreateMapStudentGradeValidator : AbstractValidator<CreateMapStudentGradeRequest>
    {
        public CreateMapStudentGradeValidator()
        {
            RuleFor(x => x.IdGrade).NotEmpty();
            RuleFor(x => x.Ids).NotNull();
        }
    }
}
