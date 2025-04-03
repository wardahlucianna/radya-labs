using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnBlocking.BlockingType;
using FluentValidation;

namespace BinusSchool.User.FnBlocking.BlockingType.Validator
{
    public class UpdateBlockingTypeValidator : AbstractValidator<UpdateBlockingTypeRequest>
    {
        public UpdateBlockingTypeValidator()
        {

            RuleFor(x => x.BlockingType).NotEmpty();

            RuleFor(x => x.IdMenu).NotEmpty();

            When(x => x.IdSubMenu is null, () => {
                RuleFor(x => x.IdMenu).Must(IsHaveSubMenu).WithMessage("Sub Menu must not be empty");
            });

        }

        public static bool IsHaveSubMenu(string idMenu)
        {
            return false;
        }
    }
}
