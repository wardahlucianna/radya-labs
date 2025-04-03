using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator
{
    public class GetDetailApprovalMeritDemeritValidator : AbstractValidator<GetDetailApprovalMeritDemeritRequest>
    {
        public GetDetailApprovalMeritDemeritValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot empty");
            RuleFor(x => x.IdUser).NotEmpty().WithMessage("Id user cannot empty");
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Academic year cannot empty");
            RuleFor(x => x.IdLevel).NotEmpty().WithMessage("Level cannot empty");
            RuleFor(x => x.IdGrade).NotEmpty().WithMessage("Grade cannot empty");
        }
    }
}
