using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.ExemplaryCharacter.Validator
{
    public class UpdateExemplaryCategorySettingsValidator : AbstractValidator<UpdateExemplaryCategorySettingsRequest>
    {
        public UpdateExemplaryCategorySettingsValidator()
        {
            RuleFor(x => x.IdExemplaryCategory).NotEmpty();
            RuleFor(x => x.ShortDesc).NotEmpty();
            RuleFor(x => x.LongDesc).NotEmpty();
            RuleFor(x => x.CurrentStatus).NotNull();
        }
    }
}
