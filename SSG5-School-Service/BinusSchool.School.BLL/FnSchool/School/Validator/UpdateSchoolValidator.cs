using BinusSchool.Data.Model.School.FnSchool.School;
using FluentValidation;

namespace BinusSchool.School.FnSchool.School.Validator
{
    public class UpdateSchoolValidator : AbstractValidator<UpdateSchoolRequest>
    {
        public UpdateSchoolValidator()
        {
            Include(new AddSchoolValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
