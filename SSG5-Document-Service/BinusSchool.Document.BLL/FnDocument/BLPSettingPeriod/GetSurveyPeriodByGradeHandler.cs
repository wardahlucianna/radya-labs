using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.BLPSettingPeriod;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Abstractions;

namespace BinusSchool.Document.FnDocument.BLPSettingPeriod
{
    public class GetSurveyPeriodByGradeHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetSurveyPeriodByGradeHandler(IDocumentDbContext dbContext,
             IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSurveyPeriodByGradeRequest>(nameof(GetSurveyPeriodByGradeRequest.IdSurveyCategory), nameof(GetSurveyPeriodByGradeRequest.IdGrade));

            int getDayToday = (int)_dateTime.ServerTime.DayOfWeek + 1;
            var SurveyPeriodGrade = await _dbContext.Entity<MsSurveyPeriod>()
                                              .Include(x => x.ClearanceWeekPeriods)
                                              .Include(x => x.ConsentCustomSchedules)
                                              .Include(x => x.Grade).ThenInclude(y => y.Level).ThenInclude(y => y.AcademicYear)
                                              .Where(a => a.IdGrade == param.IdGrade &&                                                         
                                                           a.IdSurveyCategory == param.IdSurveyCategory &&
                                                           a.StartDate <= _dateTime.ServerTime &&
                                                           a.EndDate >= _dateTime.ServerTime)
                                              .Select(a => new GetSurveyPeriodByGradeResult
                                              {                                                
                                                  IdSurveyPeriod = a.Id,
                                                  StartDate = a.StartDate,
                                                  EndDate = a.EndDate,
                                                  AllowedToEdit = (a.CustomSchedule == false ? true : ((a.ConsentCustomSchedules != null ? a.ConsentCustomSchedules.Select(b => new { RegularScheduleOpenStatus = (new DateTime(_dateTime.ServerTime.AddDays(Convert.ToInt32(b.StartDay) - getDayToday).Year, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.StartDay) - getDayToday).Month, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.StartDay) - getDayToday).Day, b.StartTime.Hours, b.StartTime.Minutes, 0) <= _dateTime.ServerTime) && (_dateTime.ServerTime <= new DateTime(_dateTime.ServerTime.AddDays(Convert.ToInt32(b.EndDay) - getDayToday).Year, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.EndDay) - getDayToday).Month, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.EndDay) - getDayToday).Day, b.EndTime.Hours, b.EndTime.Minutes, 0)) ? true : false }).First().RegularScheduleOpenStatus : false)
                                                                                                        ||
                                                                                                        (a.ClearanceWeekPeriods != null ? a.ClearanceWeekPeriods.Where(b => b.StartDate <= _dateTime.ServerTime && b.EndDate >= _dateTime.ServerTime).Count() > 0 : false)
                                                                                                       )
                                                                  ),
                                                  CustomSchedule = a.CustomSchedule,
                                                  CustomConsentSchedule = (a.CustomSchedule ? a.ConsentCustomSchedules.Select(b => new GetSurveyPeriodByGradeResult_CustomScheduleVm
                                                                                {
                                                                                      StartDate = (new DateTime(_dateTime.ServerTime.AddDays(Convert.ToInt32(b.StartDay) - getDayToday).Year, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.StartDay) - getDayToday).Month, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.StartDay) - getDayToday).Day, b.StartTime.Hours, b.StartTime.Minutes, 0)),
                                                                                      EndDate = (new DateTime(_dateTime.ServerTime.AddDays(Convert.ToInt32(b.EndDay) - getDayToday).Year, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.EndDay) - getDayToday).Month, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.EndDay) - getDayToday).Day, b.EndTime.Hours, b.EndTime.Minutes, 0)),
                                                                                      RegularScheduleOpenStatus = ((new DateTime(_dateTime.ServerTime.AddDays(Convert.ToInt32(b.StartDay) - getDayToday).Year, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.StartDay) - getDayToday).Month, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.StartDay) - getDayToday).Day, b.StartTime.Hours, b.StartTime.Minutes, 0) <= _dateTime.ServerTime) && (_dateTime.ServerTime <= new DateTime(_dateTime.ServerTime.AddDays(Convert.ToInt32(b.EndDay) - getDayToday).Year, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.EndDay) - getDayToday).Month, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.EndDay) - getDayToday).Day, b.EndTime.Hours, b.EndTime.Minutes, 0)) ? true : false),
                                                                                 }).FirstOrDefault()
                                                                                : null),
                                                  CustomClearanceSchedule = (a.CustomSchedule ? a.ClearanceWeekPeriods.Where(b => b.StartDate <= _dateTime.ServerTime && b.EndDate >= _dateTime.ServerTime).Select(c => new GetSurveyPeriodByGradeResult_ClearanceWeekVm
                                                                             {                                                      
                                                                                    WeekID = c.WeekID,
                                                                                    IdBLPGroup = c.IdBLPGroup,
                                                                                    StartDate = c.StartDate,
                                                                                    EndDate = c.EndDate
                                                                              }).ToList() : null)

                                              })
                                              .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(SurveyPeriodGrade as object);

            throw new NotImplementedException();
        }
    }
}
