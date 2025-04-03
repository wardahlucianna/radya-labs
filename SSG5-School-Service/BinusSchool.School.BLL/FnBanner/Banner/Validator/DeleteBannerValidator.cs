using BinusSchool.Data.Model.School.FnBanner.Banner;
using FluentValidation;

namespace BinusSchool.School.FnBanner.Banner.Validator
{
    public class DeleteBannerValidator : AbstractValidator<DeleteAllBannerRequest>
    {
        public DeleteBannerValidator()
        {
            RuleFor(x => x.IdBanners).NotEmpty();
        }
    }
}
