using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using FluentValidation;

namespace BinusSchool.School.FnSchool.ClassRoomMapping.Validator
{
    public class AddMappingClassValidator : AbstractValidator<AddMappingClass>
    {
        public AddMappingClassValidator()
        {
            RuleFor(x => x.IdGrade).NotEmpty();
            RuleForEach(x => x.Pathways).NotNull();
            RuleForEach(x => x.Classrooms).NotNull();

        }
    }
}
