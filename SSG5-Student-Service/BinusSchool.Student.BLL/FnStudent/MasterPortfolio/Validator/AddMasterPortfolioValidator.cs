using BinusSchool.Data.Model.Student.FnStudent.MasterPortfolio;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MasterPortfolio.Validator
{
    public class AddMasterPortfolioValidator : AbstractValidator<AddMasterPortfolioRequest>
    {
        public AddMasterPortfolioValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithName("Name");

            RuleFor(x => x.Type)
                .NotNull()
                .WithName("Type cannot null");
        }
    }
}
