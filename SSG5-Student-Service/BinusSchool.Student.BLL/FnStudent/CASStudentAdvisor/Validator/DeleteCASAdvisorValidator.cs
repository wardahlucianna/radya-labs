using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.CASStudentAdvisor;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.CASStudentAdvisor.Validator
{
    public class DeleteCASAdvisorValidator : AbstractValidator<DeleteCASAdvisorRequest>
    {
        public DeleteCASAdvisorValidator()
        {
            RuleFor(x => x.IdCasAdvisor).NotEmpty();
        }
    }
}
