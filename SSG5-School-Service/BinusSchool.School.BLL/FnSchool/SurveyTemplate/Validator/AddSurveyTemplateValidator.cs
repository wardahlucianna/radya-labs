using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.Quota;
using BinusSchool.Data.Model.School.FnSchool.SurveyTemplate;
using FluentValidation;

namespace BinusSchool.School.FnSchool.SurveyTemplate.Validator
{
    public class AddSurveyTemplateValidator : AbstractValidator<AddSurveyTemplateRequest>
    {
        public AddSurveyTemplateValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cant empty");
            RuleFor(x => x.IdTemplateChild).NotEmpty().WithMessage("Id template child cant empty");
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Id academic year cant empty");
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title cant empty");
        }
    }
}
