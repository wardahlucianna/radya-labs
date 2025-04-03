﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerReservation;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerReservation.Validator
{
    public class GetLockerListValidator : AbstractValidator<GetLockerListRequest>
    {
        public GetLockerListValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.Semester).NotNull();
            RuleFor(x => x.IdBuilding).NotEmpty();
            RuleFor(x => x.IdFloor).NotNull();
        }
    }
}
