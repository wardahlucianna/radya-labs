using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiary.Validator
{
    public class GetClassIdByTeacherRequestValidator : AbstractValidator<GetClassIdByStudentRequest>
    {
        public GetClassIdByTeacherRequestValidator()
        {
            RuleFor(x => x.AcademicYearId).NotEmpty().WithMessage("Academic year cannot empty");
            RuleFor(x => x.GradeId).NotEmpty().WithMessage("Grade cannot empty");
            RuleFor(x => x.SubjectId).NotEmpty().WithMessage("Subject cannot empty");
            RuleFor(x => x.HomeroomId).NotEmpty().WithMessage("Homeroom cannot empty");
            RuleFor(x => x.Semester).NotEmpty().WithMessage("Semester cannot empty");
            //RuleFor(x => x.UserId).NotEmpty().WithMessage("User cannot empty");
        }
    }
}
