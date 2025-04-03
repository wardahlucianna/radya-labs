using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.ExemplaryCharacter.Validator
{
    public class CreateExemplaryCategorySettingsValidator : AbstractValidator<CreateExemplaryCategorySettingsRequest>
    {
        public CreateExemplaryCategorySettingsValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.ShortDesc).NotEmpty();
            RuleFor(x => x.LongDesc).NotEmpty();
            RuleFor(x => x.CurrentStatus).NotNull();
        }
    }
}
