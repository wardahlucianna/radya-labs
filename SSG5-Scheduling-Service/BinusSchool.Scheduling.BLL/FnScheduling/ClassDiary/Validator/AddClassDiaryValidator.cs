using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiary.Validator
{
    public class AddClassDiaryValidator : AbstractValidator<AddClassDiaryRequest>
    {
        public AddClassDiaryValidator()
        {
            RuleFor(x => x.AcademicYearId).NotEmpty().WithMessage("Academic year cannot empty");
            RuleFor(x => x.GradeId).NotEmpty().WithMessage("Grade cannot empty");
            RuleFor(x => x.SubjectId).NotEmpty().WithMessage("Subject cannot empty");
            RuleFor(x => x.Semester).NotEmpty().WithMessage("Semester cannot empty");
            RuleFor(x => x.LessonId).NotEmpty().WithMessage("Lesson cannot empty");
            RuleFor(x => x.Date).NotEmpty().WithMessage("Date cannot empty");
            RuleFor(x => x.ClassDiaryTypeSettingId).NotEmpty().WithMessage("Setting Type cannot empty");
            RuleFor(x => x.Topic).NotEmpty().WithMessage("Topic cannot empty");
        }


    }
}
