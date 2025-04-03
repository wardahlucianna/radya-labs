using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoachAttendance;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.ElectiveCoachAttendance.Validator
{
   
    public class AddElectiveCoachAttendanceValidator : AbstractValidator<AddElectiveCoachAttendanceRequest>
    {
        public AddElectiveCoachAttendanceValidator()
        {
            RuleFor(x => x.IdExternalCoach).NotEmpty();           
        }
    }
}
