using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.RolePosition.Validator
{
    public class GetUserByRolePositionValidator : AbstractValidator<GetUserRolePositionRequest>
    {
        public GetUserByRolePositionValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("IdAcademicYear year cannot empty");
            RuleFor(x => x.IdSchool).NotEmpty().WithMessage("IdSchool cannot empty");
        }
    }
}
