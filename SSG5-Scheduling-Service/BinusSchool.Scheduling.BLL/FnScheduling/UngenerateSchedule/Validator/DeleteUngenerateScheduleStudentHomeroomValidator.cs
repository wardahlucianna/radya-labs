using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.UngenerateSchedule.Validator
{
    public class DeleteUngenerateScheduleStudentHomeroomValidator : AbstractValidator<DeleteUngenerateScheduleStudentHomeroomRequest>
    {
        public DeleteUngenerateScheduleStudentHomeroomValidator()
        {
            RuleFor(x => x.CodeAction).Must(x => x == "T9R").WithMessage("Wrong Code Action");
        }
    }
}
