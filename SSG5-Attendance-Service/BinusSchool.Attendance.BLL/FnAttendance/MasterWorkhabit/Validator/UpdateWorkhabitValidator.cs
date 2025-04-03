using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.MasterWorkhabit;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.MasterWorkhabit.Validator
{
    public class UpdateWorkhabitValidator : AbstractValidator<UpdateWorkhabitRequest>
    {
        public UpdateWorkhabitValidator()
        {
            Include(new AddWorkhabitValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
