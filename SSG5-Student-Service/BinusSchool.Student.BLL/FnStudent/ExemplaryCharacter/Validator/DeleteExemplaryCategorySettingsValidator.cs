using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.ExemplaryCharacter.Validator
{
    public class DeleteExemplaryCategorySettingsValidator : AbstractValidator<DeleteExemplaryCategorySettingsRequest>
    {
        public DeleteExemplaryCategorySettingsValidator()
        {
            RuleFor(x => x.IdExemplaryCategoryList).NotEmpty();
        }
    }
}
