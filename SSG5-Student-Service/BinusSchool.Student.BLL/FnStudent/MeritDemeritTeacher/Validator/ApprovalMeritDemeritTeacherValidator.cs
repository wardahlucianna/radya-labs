using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator
{
    public class ApprovalMeritDemeritTeacherValidator : AbstractValidator<ApprovalMeritDemeritTeacherRequest>
    {
        public ApprovalMeritDemeritTeacherValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Academic year cannot empty");
            RuleFor(x => x.IdLevel).NotEmpty().WithMessage("Level cannot empty");
            RuleFor(x => x.IdGrade).NotEmpty().WithMessage("Grade cannot empty");
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot empty");
            RuleFor(x => x.IdHomeroomStudent).NotEmpty().WithMessage("Homeroom Student cannot empty");
            RuleFor(x => x.IdUser).NotEmpty().WithMessage("User cannot empty");
        }
    }
}
