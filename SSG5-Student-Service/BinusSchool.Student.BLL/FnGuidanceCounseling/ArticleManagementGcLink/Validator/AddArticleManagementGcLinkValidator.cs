using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ArticleManagementGcLink;
using FluentValidation;

namespace BinusSchool.Student.FnGuidanceCounseling.ArticleManagementGcLink.Validator
{
    public class AddArticleManagementGcLinkValidator : AbstractValidator<AddArticleManagementGcLinkRequest>
    {
        public AddArticleManagementGcLinkValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();

            RuleFor(x => x.GradeIds).NotEmpty();

            RuleFor(x => x.LinkDescription).NotEmpty();

            RuleFor(x => x.Link).NotEmpty();

            RuleFor(x => x.Link).Must(StringUtil.IsValidUrl).WithMessage("Invalid link");
        }
    }
}
