using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.BLPSettingPeriod;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.BLPSettingPeriod.Validator
{
    public class UpdateBLPSettingPeriodValidator : AbstractValidator<UpdateBLPSettingPeriodRequest>
    {
        public UpdateBLPSettingPeriodValidator()
        {
            RuleFor(x => x.IdSurveyPeriod).NotEmpty();
            RuleFor(x => x.IdSurveyCategory).NotEmpty(); 
            RuleFor(x => x.IdGrade).NotEmpty();
            RuleFor(x => x.StartDateSurvey).NotEmpty();
            RuleFor(x => x.EndDateSurvey).NotEmpty().GreaterThan(x => x.StartDateSurvey).WithMessage("End date must be greater than start date");
            When(x => x.HasConsentCustomSchedule == true, () =>
            {
                RuleFor(x => x.ConsentCustomSchedule).NotEmpty();
                RuleFor(x => x.ConsentCustomSchedule.StartDay).NotEmpty();
                RuleFor(x => x.ConsentCustomSchedule.EndDay).GreaterThan(x => x.ConsentCustomSchedule.StartDay).WithMessage("End day must be greater than start day");
                RuleFor(x => x.ConsentCustomSchedule.StartTime).NotEmpty();
                RuleFor(x => x.ConsentCustomSchedule.EndTime).NotEmpty();
            });
            When(x => x.HasClearanceWeekPeriod == true, () =>
            {
                RuleFor(x => x.ClearanceWeekPeriod)
                  .NotEmpty()
                  .ForEach(data => data.ChildRules(data =>
                  {
                      data.RuleFor(x => x.StartDate).NotEmpty();
                      data.RuleFor(x => x.EndDate).NotEmpty();
                      data.RuleFor(x => x.WeekID).NotEmpty();
                      data.RuleFor(x => x.IdBLPGroup).NotEmpty();
                  }));
            });
        }
    }
}
