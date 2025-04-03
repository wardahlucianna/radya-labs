using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using FluentValidation;
using System.Linq;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent.Validator
{
    public class DeleteSchooEvenetInvolvementValidator : AbstractValidator<DeleteSchoolEventInvolvementRequest>
    {
        public DeleteSchooEvenetInvolvementValidator()
        {

            RuleFor(x => x.IdEvent).NotNull();
            
            RuleFor(x => x.UserId).NotNull();
        }
    }
}
