using System;
using System.Collections.Generic;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.Student.FnStudent.SiblingGroup;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.SiblingGroup.Validator
{
    public class AddSiblingGroupValidator : AbstractValidator<AddSiblingGroupRequest>
    {
        public AddSiblingGroupValidator()
        {
            RuleFor(x => x.IdSiblingGroup).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();
            //RuleFor(x => x.IsParentUpdate).NotEmpty();
        }
    }
}
