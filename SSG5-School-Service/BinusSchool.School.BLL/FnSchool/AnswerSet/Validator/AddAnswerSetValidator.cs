using BinusSchool.Data.Model.School.FnSchool.AnswerSet;
using FluentValidation;

namespace BinusSchool.School.FnSchool.AnswerSet.Validator
{
    public class AddAnswerSetValidator : AbstractValidator<AddAnswerSetRequest>
    {
        public AddAnswerSetValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.AnswerSetName).NotEmpty();
        }
    }
}
