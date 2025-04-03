using BinusSchool.Common.Model;
using FluentValidation;

namespace BinusSchool.Common.Validators
{
    public class CodeVmValidatorOf<T, TCode> : AbstractValidator<T> 
        where T : CodeVm<TCode>
    {
        public CodeVmValidatorOf()
        {
            RuleFor(x => x.Code).NotEmpty();
        }
    }

    public class CodeVmValidator<T> : AbstractValidator<T> 
        where T : CodeVm
    {
        public CodeVmValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .MinimumLength(1);
        }
    }
}
