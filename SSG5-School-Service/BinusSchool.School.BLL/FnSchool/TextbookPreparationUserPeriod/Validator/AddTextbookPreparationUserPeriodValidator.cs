﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationUserPeriod;
using FluentValidation;

namespace BinusSchool.School.FnSchool.TextbookPreparationUserPeriod.Validator
{
    public class AddTextbookPreparationUserPeriodValidator : AbstractValidator<AddTextbookPreparationUserPeriodRequest>
    {
        public AddTextbookPreparationUserPeriodValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Academic year cant empty");
            RuleFor(x => x.GroupName).NotEmpty().WithMessage("Group name Year cant empty");
            RuleFor(x => x.UserStaff)
                .ForEach(datas => datas.ChildRules(data =>
                {
                    data.RuleFor(e => e.IdRole).NotEmpty();
                    data.RuleFor(e => e.IdPosition).NotEmpty();
                    data.RuleFor(e => e.IdUser).NotEmpty();
                })
            );
        }
    }
}
