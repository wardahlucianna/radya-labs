using BinusSchool.Common.Model;
using FluentValidation;

namespace BinusSchool.Common.Validators
{
    public class CodeWithIdVmValidatorOf<T, TCode> : AbstractValidator<T>
        where T : CodeWithIdVm<TCode>
    {
        public CodeWithIdVmValidatorOf()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Code).NotEmpty();
        }
    }

    public class CodeWithIdVmValidator<T> : AbstractValidator<T>
        where T : CodeWithIdVm
    {
        public CodeWithIdVmValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Code).NotEmpty();
        }
    }
}