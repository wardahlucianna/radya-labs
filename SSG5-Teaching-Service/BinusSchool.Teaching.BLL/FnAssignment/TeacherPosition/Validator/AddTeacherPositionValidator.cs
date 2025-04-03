using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPosition;
using FluentValidation;

namespace BinusSchool.Teaching.FnAssignment.TeacherPosition.Validator
{
    public class AddTeacherPositionValidator : AbstractValidator<AddTeacherPositionRequest>
    {
        public AddTeacherPositionValidator()
        {
            RuleFor(p => p.Alias).NotEmpty();

            RuleFor(p => p.IdSchool).NotEmpty();

            RuleFor(p => p.Code)
                .NotEmpty()
                .WithName("Position Short Name");
            
            RuleFor(x => x.Description)
                .NotEmpty()
                .WithName("Position Name");
            
            RuleFor(x=>x.IdPosition).NotEmpty().WithName("Position");
        }
    }
}
