using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.RolePosition.Validator
{
    public class GetSubjectByUserValidator : AbstractValidator<GetSubjectByUserRequest>
    {
        public GetSubjectByUserValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("IdAcademicYear year cannot empty");
            RuleFor(x => x.IdUser).NotEmpty().WithMessage("IdUser cannot empty");
        }
    }
}
