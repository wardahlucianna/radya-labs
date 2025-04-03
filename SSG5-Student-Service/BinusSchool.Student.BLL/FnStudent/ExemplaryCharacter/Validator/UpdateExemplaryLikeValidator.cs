using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.ExemplaryCharacter.Validator
{
    public class UpdateExemplaryLikeValidator : AbstractValidator<UpdateExemplaryLikeRequest>
    {
        public UpdateExemplaryLikeValidator()
        {
            RuleFor(x => x.IdUser).NotEmpty();
            RuleFor(x => x.idExemplary).NotEmpty();
        }
    }
}
