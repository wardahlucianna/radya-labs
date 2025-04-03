using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolVisitor;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.SchoolVisitor.Validator
{
    public class UpdateSchoolVisitorValidator : AbstractValidator<UpdateSchoolVisitorRequest>
    {
        public UpdateSchoolVisitorValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.VisitorDate).NotEmpty();
            RuleFor(x => x.IdVenue).NotEmpty();
        }
    }
}
