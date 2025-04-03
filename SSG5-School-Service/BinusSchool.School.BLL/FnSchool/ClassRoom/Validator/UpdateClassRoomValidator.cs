using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.School.FnSchool.ClassRoom;
using FluentValidation;

namespace BinusSchool.School.FnSchool.ClassRoom.Validator
{
    public class UpdateClassRoomValidator : AbstractValidator<UpdateClassRoomRequest>
    {
        public UpdateClassRoomValidator()
        {
            Include(new AddClassRoomValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
