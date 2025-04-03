using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc;
using FluentValidation;

namespace BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc.Validator
{
    public class DeleteReportStudentToGcValidator : AbstractValidator<DeleteReportStudentToGcRequest>
    {
        public DeleteReportStudentToGcValidator()
        {
            RuleFor(x => x.Ids).NotEmpty().WithMessage("Id cannot empty");
        }

    }
}
