using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.StudentInfoUpdate;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.StudentInfoUpdate.Validator
{
    class StudentInfoUpdateValidator : AbstractValidator<GetStudentInfoUpdateRequest>
    {
        public StudentInfoUpdateValidator()
        {
        }
    }
}
