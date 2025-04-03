using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using FluentValidation;

namespace BinusSchool.User.FnBlocking.StudentBlocking.Validator
{
    public class UpdateStudentUnBlockingValidator : AbstractValidator<UpdateStudentUnBlockingRequest>
    {
        public UpdateStudentUnBlockingValidator()
        {
            RuleFor(x => x.IdBlockingCategory).NotNull();
            RuleFor(x => x.IdBlockingType).NotNull();
            RuleForEach(x => x.IdUsers).NotEmpty();
        }
    }
}
