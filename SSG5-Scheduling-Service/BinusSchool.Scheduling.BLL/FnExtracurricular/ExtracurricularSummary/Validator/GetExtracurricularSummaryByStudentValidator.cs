using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularSummary;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularSummary.Validator
{
    public class GetExtracurricularSummaryByStudentValidator : AbstractValidator<GetExtracurricularSummaryByStudentRequest>
    {
        public GetExtracurricularSummaryByStudentValidator()
        {
            //RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.IdGrade).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();
        }
    }
}
