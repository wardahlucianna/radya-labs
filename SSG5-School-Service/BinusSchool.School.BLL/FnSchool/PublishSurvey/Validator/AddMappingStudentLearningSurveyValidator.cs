using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.PublishSurvey;
using FluentValidation;

namespace BinusSchool.School.FnSchool.PublishSurvey.Validator
{
    public class AddMappingStudentLearningSurveyValidator : AbstractValidator<AddMappingStudentLearningSurveyRequest>
    {
        public AddMappingStudentLearningSurveyValidator()
        {
            RuleFor(x => x.IdPublishSurvey).NotEmpty().WithMessage("Id publish survey cant empty");
            RuleFor(x => x.IdHomeroom).NotEmpty().WithMessage("Id Homeroom cant empty");
        }
    }
}
