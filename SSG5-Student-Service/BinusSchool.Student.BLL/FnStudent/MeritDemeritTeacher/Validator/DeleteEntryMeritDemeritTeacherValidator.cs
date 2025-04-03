using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using FluentValidation;


namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator
{
    public class DeleteEntryMeritDemeritTeacherValidator : AbstractValidator<DeleteEntryMeritDemeritTeacherRequest>
    {
        public DeleteEntryMeritDemeritTeacherValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot empty");
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Academic Year cannot empty");
        }

    }
}
