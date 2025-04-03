using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationApprovalSetting;
using FluentValidation;

namespace BinusSchool.School.FnSchool.TextbookPreparationApprovalSetting.Validator
{
    public class AddTextbookPreparationApprovalSettingValidator : AbstractValidator<AddTextbookPreparationApprovalSettingRequest>
    {
        public AddTextbookPreparationApprovalSettingValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty().WithMessage("Id school cant empty");
            RuleFor(x => x.IdSchool).NotEmpty().WithMessage("Id school cant empty");
            RuleFor(x => x.TextbookPreparationApprovalSetting)
                .ForEach(datas => datas.ChildRules(data =>
                {
                    data.RuleFor(e => e.IdUser).NotEmpty().WithMessage("Id user cant empty");
                    data.RuleFor(e => e.IdRole).NotEmpty().WithMessage("Id role cant empty");
                    data.RuleFor(e => e.IdTeacherPosition).NotEmpty().WithMessage("Id teacher position cant empty");
                    data.RuleFor(e => e.ApproverTo).NotEmpty().WithMessage("Approval to cant empty");
                })
            );
        }
    }
}
