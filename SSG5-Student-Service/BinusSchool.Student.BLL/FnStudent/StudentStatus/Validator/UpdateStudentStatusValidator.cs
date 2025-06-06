﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.StudentStatus;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.StudentStatus.Validator
{
    public class UpdateStudentStatusValidator : AbstractValidator<UpdateStudentStatusRequest>
    {
        public UpdateStudentStatusValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();
            RuleFor(x => x.IdStudentStatus).NotEmpty();
            RuleFor(x => x.NewStatusStartDate).NotEmpty();
        }
    }
}
