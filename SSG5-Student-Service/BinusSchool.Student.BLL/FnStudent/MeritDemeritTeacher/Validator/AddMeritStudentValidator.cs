using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator
{
    public class AddMeritStudentValidator : AbstractValidator<AddMeritStudentRequest>
    {
        public AddMeritStudentValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Academic year cannot empty");
            RuleFor(x => x.MeritStudents).NotEmpty().WithMessage("Merit Student cannot empty");
        }
    }
}
