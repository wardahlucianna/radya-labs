using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Student.Validator
{
    public class GetStudentInformationForBNSReportValidator : AbstractValidator<GetStudentInformationForBNSReportRequest>
    {
        public GetStudentInformationForBNSReportValidator()
        {
            RuleFor(x => x.IdStudent).NotEmpty();
            RuleFor(x => x.IdGrade).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
        }
    }
}
