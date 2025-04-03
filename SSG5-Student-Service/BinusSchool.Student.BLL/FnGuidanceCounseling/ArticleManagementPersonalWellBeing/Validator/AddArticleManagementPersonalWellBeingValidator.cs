using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ArticleManagementPersonalWellBeing;
using FluentValidation;

namespace BinusSchool.Student.FnGuidanceCounseling.ArticleManagementPersonalWellBeing.Validator
{
    public class AddArticleManagementPersonalWellBeingValidator : AbstractValidator<AddArticleManagementPersonalWellBeingRequest>
    {
        public AddArticleManagementPersonalWellBeingValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();

            RuleFor(x => x.LevelIds).NotEmpty();

            RuleFor(x => x.ArticleName).NotEmpty();

            When(x => !string.IsNullOrEmpty(x.Link), () => {
                RuleFor(x => x.Link).Must(StringUtil.IsValidUrl).WithMessage("Invalid link");
            });


        }
    }
}
