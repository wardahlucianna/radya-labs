using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc;
using FluentValidation;

namespace BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc.Validator
{
    public class ReadStudentToGcValidator : AbstractValidator<ReadStudentToGcRequest>
    {
        public ReadStudentToGcValidator()
        {
            RuleFor(x => x.IdGcReportStudent).NotEmpty().WithMessage("Id cannot empty");
        }
    }
}
