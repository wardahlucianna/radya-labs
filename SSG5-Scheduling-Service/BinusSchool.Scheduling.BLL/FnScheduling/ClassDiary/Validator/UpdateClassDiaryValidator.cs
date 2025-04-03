using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using FluentValidation;
namespace BinusSchool.Scheduling.FnSchedule.ClassDiary.Validator
{
    public class UpdateClassDiaryValidator : AbstractValidator<UpdateClassDiaryRequest>
    {
        public UpdateClassDiaryValidator()
        {
            RuleFor(x => x.ClassDiaryId).NotEmpty().WithMessage("Class diary id cannot empty");
            RuleFor(x => x.Date).NotEmpty().WithMessage("Date cannot empty");
            RuleFor(x => x.ClassDiaryTypeSettingId).NotEmpty().WithMessage("Setting Type cannot empty");
            RuleFor(x => x.Topic).NotEmpty().WithMessage("Topic cannot empty");
        }


    }

}
