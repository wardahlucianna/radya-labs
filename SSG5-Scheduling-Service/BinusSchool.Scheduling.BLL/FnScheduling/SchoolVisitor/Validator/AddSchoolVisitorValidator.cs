using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolVisitor;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.SchoolVisitor.Validator
{
    public class AddSchoolVisitorValidator : AbstractValidator<AddSchoolVisitorRequest>
    {
        public AddSchoolVisitorValidator()
        {
            RuleFor(x => x.VisitorDate).NotEmpty();
            RuleFor(x => x.IdVenue).NotEmpty();
            RuleFor(x => x.IdAcademicYear).NotEmpty();
        }
    }
}
