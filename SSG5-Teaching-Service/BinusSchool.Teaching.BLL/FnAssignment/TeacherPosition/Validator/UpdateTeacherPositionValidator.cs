using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPosition;
using FluentValidation;

namespace BinusSchool.Teaching.FnAssignment.TeacherPosition.Validator
{
    public class UpdateTeacherPositionValidator : AbstractValidator<UpdateTeacherPositionRequest>
    {
        public UpdateTeacherPositionValidator()
        {
            Include(new AddTeacherPositionValidator());

            RuleFor(p => p.Id).NotEmpty();
        }
    }
}
