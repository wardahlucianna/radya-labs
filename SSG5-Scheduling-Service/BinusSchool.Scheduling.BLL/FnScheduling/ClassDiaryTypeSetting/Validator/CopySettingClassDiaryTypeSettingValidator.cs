using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiaryTypeSetting;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiaryTypeSetting.Validator
{
    public class CopySettingClassDiaryTypeSettingValidator : AbstractValidator<CopySettingClassDiaryTypeSettingRequest>
    {
        public CopySettingClassDiaryTypeSettingValidator()
        {
            RuleFor(x => x.IdAcademicYearCopyTo).NotEmpty();
            RuleFor(x => x.IdClassDiaryTypeSettings).NotEmpty();
        }
    }
}
