using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using FluentValidation;


namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator
{
    public class GetExsisEntryMeritDemeritTeacherByIdValidator : AbstractValidator<GetExsisEntryMeritDemeritTeacherByIdRequest>
    {
        public GetExsisEntryMeritDemeritTeacherByIdValidator()
        {
            RuleFor(x => x.IdMeritDemerit).NotEmpty().WithMessage("Code Merit Demerit cannot empty");
        }
    }
}
