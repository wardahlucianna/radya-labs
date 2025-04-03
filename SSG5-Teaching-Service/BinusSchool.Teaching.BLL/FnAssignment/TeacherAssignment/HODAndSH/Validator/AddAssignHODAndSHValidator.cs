using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment.HODAndSH;
using FluentValidation;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignment.HODAndSH.Validator
{
    public class AddAssignHODAndSHValidator : AbstractValidator<AddAssignHODAndSHRequest>
    {
        public AddAssignHODAndSHValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.IdSchoolAcadYear).NotEmpty();
        }
    }
}
