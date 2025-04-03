using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparation;
using FluentValidation;

namespace BinusSchool.School.FnSchool.TextbookPreparation.Validator
{
    public class AddUploadTextbookPreparationValidator : AbstractValidator<AddUploadTextbookPreparationRequest>
    {
        public AddUploadTextbookPreparationValidator()
        {
            RuleFor(x => x.TextbookPeparations)
                .ForEach(datas => datas.ChildRules(data =>
                {
                    data.RuleFor(x => x.IdUser).NotEmpty().WithMessage("Id user cant empty");
                    data.RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Id academic cant empty");
                    data.RuleFor(x => x.IdSubject).NotEmpty().WithMessage("Id subject cant empty");
                    data.RuleFor(x => x.ISBN).NotEmpty().WithMessage("ISBN cant empty");
                    data.RuleFor(x => x.Title).NotEmpty().WithMessage("Title cant empty");
                    data.RuleFor(x => x.Author).NotEmpty().WithMessage("Author cant empty");
                })
            );
        }
    }
}
