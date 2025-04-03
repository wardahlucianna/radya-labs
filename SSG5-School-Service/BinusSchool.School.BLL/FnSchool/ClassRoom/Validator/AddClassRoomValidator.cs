using System;
using System.Collections.Generic;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.School.FnSchool.ClassRoom;
using FluentValidation;

namespace BinusSchool.School.FnSchool.ClassRoom.Validator
{
    public class AddClassRoomValidator : AbstractValidator<AddClassRoomRequest>
    {
        public AddClassRoomValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithName("Class Alias");
            
            RuleFor(x => x.Description)
                .NotEmpty()
                .WithName("Class Name");
        }
    }
}
