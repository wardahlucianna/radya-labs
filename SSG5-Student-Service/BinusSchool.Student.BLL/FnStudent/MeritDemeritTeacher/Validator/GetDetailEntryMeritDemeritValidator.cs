using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator
{
    public class GetDetailEntryMeritDemeritValidator : AbstractValidator<GetDetailEntryMeritDemeritRequest>
    {
        public GetDetailEntryMeritDemeritValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot empty");
        }
    }
}
