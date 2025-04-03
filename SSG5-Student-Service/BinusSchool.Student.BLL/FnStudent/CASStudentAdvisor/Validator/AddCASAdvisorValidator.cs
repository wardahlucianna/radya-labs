using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.CASStudentAdvisor;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.CASStudentAdvisor.Validator
{
    public class AddCASAdvisorValidator : AbstractValidator<AddCASAdvisorRequest>
    {
        public AddCASAdvisorValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.IdUser).NotEmpty();
        }
    }
}
