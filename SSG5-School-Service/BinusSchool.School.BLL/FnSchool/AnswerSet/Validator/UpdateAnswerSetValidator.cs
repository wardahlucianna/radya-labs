using BinusSchool.Data.Model.School.FnSchool.AnswerSet;
using FluentValidation;

namespace BinusSchool.School.FnSchool.AnswerSet.Validator
{
    public class UpdateAnswerSetValidator : AbstractValidator<UpdateAnswerSetRequest>
    {
        public UpdateAnswerSetValidator()
        {
            Include(new AddAnswerSetValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
