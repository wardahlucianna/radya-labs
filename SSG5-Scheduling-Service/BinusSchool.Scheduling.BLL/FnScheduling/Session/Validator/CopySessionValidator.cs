using BinusSchool.Data.Model.Scheduling.FnSchedule.Session;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Session.Validator
{
    public class CopySessionValidator : AbstractValidator<CopySessionRequest>
    {
        public CopySessionValidator()
        {
            RuleFor(x => x.IdSessionSetFrom).NotEmpty();
            RuleFor(x => x.IdSessionSetTo).NotEmpty();
        }
    }
}
