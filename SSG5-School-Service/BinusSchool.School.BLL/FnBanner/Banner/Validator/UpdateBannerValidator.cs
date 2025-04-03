using BinusSchool.Data.Model.School.FnBanner.Banner;
using FluentValidation;

namespace BinusSchool.School.FnBanner.Banner.Validator
{
    public class UpdateBannerValidator : AbstractValidator<UpdateBannerRequest>
    {
        public UpdateBannerValidator()
        {
            Include(new AddBannerValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
