using BinusSchool.Data.Model.Student.FnStudent.MasterPortfolio;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MasterPortfolio.Validator
{
    public class UpdateMasterPortfolioValidator : AbstractValidator<UpdateMasterPortfolioRequest>
    {
        public UpdateMasterPortfolioValidator()
        {
            RuleFor(x => x.Id)
                .NotNull()
                .WithName("Id cannot null");
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithName("Name");

            RuleFor(x => x.Type)
                .NotNull()
                .WithName("Type cannot null");
        }
    }
}
