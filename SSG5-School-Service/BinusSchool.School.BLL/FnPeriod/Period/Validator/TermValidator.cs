using System;
using System.Collections.Generic;
using System.Linq;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using FluentValidation;

namespace BinusSchool.School.FnPeriod.Period.Validator
{
    public class TermValidator : AbstractValidator<Term>
    {
        private readonly IList<Term> _terms;

        public TermValidator(IEnumerable<Term> terms)
        {
            _terms = terms.ToList();

            RuleFor(x => x.StartDate)
                .NotEmpty()
                .GreaterThan(x => PrevStartDate(x))
                .LessThan(x => x.EndDate);

            RuleFor(x => x.EndDate)
                .NotEmpty()
                .GreaterThan(x => x.StartDate);

            RuleFor(x => x.AttendanceStartDate)
                .NotEmpty()
                .GreaterThanOrEqualTo(x => x.StartDate)
                .LessThan(x => x.AttendanceEndDate);

            RuleFor(x => x.AttendanceEndDate)
                .NotEmpty()
                .GreaterThan(x => x.AttendanceStartDate)
                .LessThanOrEqualTo(x => x.EndDate);

            When(x => HavePrevSemester(x), () =>
            {
                RuleFor(x => x.Semester)
                    .NotEmpty()
                    .GreaterThanOrEqualTo(x => PrevSemester(x))
                    .LessThanOrEqualTo(2);
            })
            .Otherwise(() =>
            {
                When(x => x.Semester.HasValue, () =>
                {
                    RuleFor(x => x.Semester)
                        .GreaterThanOrEqualTo(1)
                        .LessThanOrEqualTo(2);
                });
            });
        }

        private DateTime PrevStartDate(Term term)
        {
            var index = _terms.IndexOf(term);

            return index != 0
                ? _terms[index - 1].StartDate
                : DateTime.MinValue;
        }

        private bool HavePrevSemester(Term term)
        {
            var index = _terms.IndexOf(term);

            return index != 0
                ? _terms[index - 1].Semester.HasValue
                : false;
        }

        private int PrevSemester(Term term)
        {
            var index = _terms.IndexOf(term);

            return index != 0
                ? _terms[index - 1].Semester.HasValue
                    ? _terms[index - 1].Semester.Value
                    : 1
                : 1;
        }
    }
}
