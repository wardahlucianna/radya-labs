using BinusSchool.Data.Model.School.FnSchool.Pathway;
using FluentValidation;

namespace BinusSchool.School.FnSchool.Pathway.Validator
{
    public class UpdatePathwayValidator : AbstractValidator<UpdatePathwayRequest>
    {
        public UpdatePathwayValidator()
        {
            Include(new AddPathwayValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
