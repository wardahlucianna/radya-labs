using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.EmailRecepient;
using FluentValidation;

namespace BinusSchool.Scheduling.FnMovingStudent.EmailRecepient.Validator
{
    public class AddEmailRecepientValidator : AbstractValidator<AddEmailRecepientRequest>
    {
        public AddEmailRecepientValidator()
        {
            RuleFor(x => x.Tos).NotEmpty().WithMessage("Id Role to cannot empty");

            RuleForEach(x => x.Tos).ChildRules(schedules => {
                schedules.RuleFor(x => x.IdRole).NotEmpty().WithMessage("Id role to cannot empty"); 
            });

        }
    }
}
