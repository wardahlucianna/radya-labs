using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MedicalSystem.Validator
{
    public class DeleteMedicalDataValidator : AbstractValidator<IdCollection>
    {
        public DeleteMedicalDataValidator()
        {
            RuleFor(x => x.Ids).NotEmpty();
        }
    }
}
