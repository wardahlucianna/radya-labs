using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.StudentStatus;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.StudentStatus.Validator
{
    public class GenerateStudentStatusMappingActiveAYValidator : AbstractValidator<GenerateStudentStatusMappingActiveAYRequest>
    {
        public GenerateStudentStatusMappingActiveAYValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.IdAcademicYear).NotEmpty();
        }
    }
}
