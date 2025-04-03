using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiary.Validator
{
    public class DeteleClassDiaryValidator : AbstractValidator<DeleteClassDiaryRequest>
    {
        public DeteleClassDiaryValidator()
        {
            RuleFor(x => x.ClassDiaryId).NotEmpty().WithMessage("Class diary id cannot empty");
            RuleFor(x => x.DeleteReason).NotEmpty().WithMessage("Reason cannot empty");
        }


    }
}
