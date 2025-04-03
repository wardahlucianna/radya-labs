using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnBlocking.BlockingCategory;
using FluentValidation;

namespace BinusSchool.User.FnBlocking.BlockingCategory.Validator
{
    public class AddBlockingCategoryValidator : AbstractValidator<AddBlockingCategoryRequest>
    {
        public AddBlockingCategoryValidator()
        {

            RuleFor(x => x.BlockingCategory).NotEmpty();

            RuleFor(x => x.IdsBlockingType).NotEmpty();

            RuleFor(x => x.IdsAssignUser).NotEmpty();

        }
    }
}
