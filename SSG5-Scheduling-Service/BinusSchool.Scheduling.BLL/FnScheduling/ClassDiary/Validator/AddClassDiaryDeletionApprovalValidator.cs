using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiary.Validator
{
    public class AddClassDiaryDeletionApprovalValidator : AbstractValidator<AddClassDiaryDeletionApprovalRequest>
    {
        public AddClassDiaryDeletionApprovalValidator()
        {
            RuleFor(x => x.ClassDiaryId).NotEmpty().WithMessage("Id cannot empty");
        }
    }
}
