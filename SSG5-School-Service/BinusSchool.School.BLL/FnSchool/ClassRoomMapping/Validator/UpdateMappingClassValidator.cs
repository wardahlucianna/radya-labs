using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using FluentValidation;

namespace BinusSchool.School.FnSchool.ClassRoomMapping.Validator
{
    public class UpdateMappingClassValidator : AbstractValidator<UpdateMappingClass>
    {
        public UpdateMappingClassValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.IdGrade).NotEmpty();
            RuleForEach(x => x.Pathways).NotNull();
            RuleForEach(x => x.Classrooms).NotNull();
        }
    }
}
