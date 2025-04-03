using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using FluentValidation;

namespace BinusSchool.School.FnPeriod.Period.Validator
{
    public class GetDateBySemesterValidator : AbstractValidator<GetDateBySemesterRequest>
    {
        public GetDateBySemesterValidator()
        {
            RuleFor(x => x.IdGrade).NotEmpty();

            RuleFor(x => x.Semester).NotEmpty();
        }
    }
}
