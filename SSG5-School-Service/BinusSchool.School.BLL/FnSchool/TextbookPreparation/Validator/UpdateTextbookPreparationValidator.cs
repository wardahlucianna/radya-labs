using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparation;
using FluentValidation;

namespace BinusSchool.School.FnSchool.TextbookPreparation.Validator
{
    public class UpdateTextbookPreparationValidator : AbstractValidator<UpdateTextbookPreparationRequest>
    {
        public UpdateTextbookPreparationValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cant empty");
            RuleFor(x => x.IdUser).NotEmpty().WithMessage("Id user cant empty");
            RuleFor(x => x.IdSubject).NotEmpty().WithMessage("Id subject cant empty");
            RuleFor(x => x.ISBN).NotEmpty().WithMessage("ISBN cant empty");
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title cant empty");
            RuleFor(x => x.Author).NotEmpty().WithMessage("Author cant empty");
        }
    }
}


