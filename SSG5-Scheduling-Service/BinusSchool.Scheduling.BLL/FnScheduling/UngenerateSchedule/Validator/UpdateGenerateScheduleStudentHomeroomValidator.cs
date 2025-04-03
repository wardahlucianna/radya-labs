using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.UngenerateSchedule.Validator
{
    public class UpdateGenerateScheduleStudentHomeroomValidator : AbstractValidator<UpdateGenerateScheduleStudentHomeroomRequest>
    {
        public UpdateGenerateScheduleStudentHomeroomValidator()
        {
            RuleFor(x => x.Start).NotEmpty();
            RuleFor(x => x.IdHomeroom).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();
        }
    }
}
