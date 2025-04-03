using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Student.Validator
{
    public class GetStudentForSalesForceValidator : AbstractValidator<GetStudentForSalesForceRequest>
    {
        public GetStudentForSalesForceValidator()
        {
            //RuleFor(x => x.IdStudent).NotEmpty();
        }
    }
}
