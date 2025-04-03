using BinusSchool.Data.Model.School.FnBanner.Banner;
using FluentValidation;

namespace BinusSchool.School.FnBanner.Banner.Validator
{
    public class AddBannerValidator : AbstractValidator<AddBannerRequest>
    {
        public AddBannerValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.ImageUrl).NotEmpty();
            RuleFor(x => x.RoleGroupId).NotEmpty();
            RuleFor(x => x.PublishStartDate).NotEmpty();
            RuleFor(x => x.PublishEndDate).NotEmpty();
            RuleFor(x => x.Option).IsInEnum();
            RuleFor(x => x.IdSchool).NotEmpty();
        }
    }
}
