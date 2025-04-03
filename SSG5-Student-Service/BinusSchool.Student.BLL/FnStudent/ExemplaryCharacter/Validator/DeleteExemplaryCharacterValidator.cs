using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.ExemplaryCharacter.Validator
{
    public class DeleteExemplaryCharacterValidator : AbstractValidator<DeleteExemplaryCharacterRequest>
    {
        public DeleteExemplaryCharacterValidator()
        {
            RuleFor(x => x.IdExemplaryCharacter).NotEmpty();
        }
    }
}
