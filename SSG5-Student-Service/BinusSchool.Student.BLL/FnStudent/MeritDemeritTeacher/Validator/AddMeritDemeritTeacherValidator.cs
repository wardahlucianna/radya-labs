using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator
{
    public class AddMeritDemeritTeacherValidator : AbstractValidator<AddMeritDemeritTeacherRequest>
    {
        public AddMeritDemeritTeacherValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Academic year cannot empty");
            RuleFor(x => x.IdLevel).NotEmpty().WithMessage("Level cannot empty");
            RuleFor(x => x.IdGrade).NotEmpty().WithMessage("Grade cannot empty");
            RuleFor(x => x.Date).NotEmpty().WithMessage("Date cannot empty");
            RuleFor(x => x.MeritDemeritTeacher).NotEmpty().WithMessage("Merit demerit  cannot empty");
            RuleFor(x => x.IdMeritDemeritMapping).NotEmpty().WithMessage("Merit demerit mapping cannot empty");
            RuleFor(x => x.NameMeritDemeritMapping).NotEmpty().WithMessage("Nama merit demerit mapping cannot empty");
        }
    }
}
