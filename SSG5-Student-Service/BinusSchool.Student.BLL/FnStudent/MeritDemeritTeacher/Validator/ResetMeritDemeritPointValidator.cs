using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator
{
    public class ResetMeritDemeritPointValidator : AbstractValidator<ResetMeritDemeritPointRequest>
    {
        public ResetMeritDemeritPointValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty().WithMessage("School cannot empty");
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Academic year cannot empty");
            RuleFor(x => x.IdGrade).NotEmpty().WithMessage("Grade cannot empty");
            RuleFor(x => x.CodeGrade).NotEmpty().WithMessage("Code garde cannot empty");
            RuleFor(x => x.Semester).NotEmpty().WithMessage("Semester cannot empty");
        }
    }
}
