using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc;
using FluentValidation;


namespace BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc.Validator
{
    public class UpdateReportStudentToGcValidator : AbstractValidator<UpdateReportStudentToGcRequest>
    {
        public UpdateReportStudentToGcValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot empty");
            RuleFor(x => x.Date).NotEmpty().WithMessage("Date cannot empty");
            RuleFor(x => x.Note).NotEmpty().WithMessage("Note cannot empty");
        }

    }
}
