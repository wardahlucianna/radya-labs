using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc;
using FluentValidation;

namespace BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc.Validator
{
    public class AddReportStudentToGcValidator : AbstractValidator<AddReportStudentToGcRequest>
    {
        public AddReportStudentToGcValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Academic year cannot empty");
            RuleFor(x => x.Date).NotEmpty().WithMessage("Date cannot empty");
            //RuleFor(x => x.IdStudent).NotEmpty().WithMessage("Student cannot empty");
            //RuleFor(x => x.IdUserReport).NotEmpty().WithMessage("User Report cannot empty");
            //RuleFor(x => x.Note).NotEmpty().WithMessage("Note cannot empty");
            //RuleFor(x => x.IdGrade).NotEmpty().WithMessage("Grade cannot empty");
        }
    }
}
