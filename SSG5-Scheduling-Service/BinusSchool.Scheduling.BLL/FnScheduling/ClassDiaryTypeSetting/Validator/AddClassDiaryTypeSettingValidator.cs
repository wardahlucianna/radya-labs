using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiaryTypeSetting;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiaryTypeSetting.Validator
{
    public class AddClassDiaryTypeSettingValidator : AbstractValidator<AddClassDiaryTypeSettingRequest>
    {
        public AddClassDiaryTypeSettingValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();

            RuleFor(x => x.TypeName).NotEmpty();

            RuleFor(x => x.OccurrencePerDay).NotEmpty().GreaterThanOrEqualTo(1).LessThanOrEqualTo(99);

            RuleFor(x => x.MinimumStartDay).NotEmpty().GreaterThanOrEqualTo(0);
        }

    }
}
