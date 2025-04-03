using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Student.Validator
{
    public class GetStudentInformationChangesHistoryValidator : AbstractValidator<GetStudentInformationChangesHistoryRequest>
    {
        public GetStudentInformationChangesHistoryValidator()
        {
            RuleFor(x => x.IdStudent).NotEmpty();
        }
    }
}
