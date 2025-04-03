using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.RolePosition.Validator
{
    public class GetUserSubjectByEmailRecepientValidator : AbstractValidator<GetUserSubjectByEmailRecepientRequest>
    {
        public GetUserSubjectByEmailRecepientValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("IdAcademicYear year cannot empty");
            RuleFor(x => x.IdSchool).NotEmpty().WithMessage("IdSchool cannot empty");
        }
    }
}
