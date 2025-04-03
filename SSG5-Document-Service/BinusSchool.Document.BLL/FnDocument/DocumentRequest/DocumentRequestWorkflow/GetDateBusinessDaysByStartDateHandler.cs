using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow
{
    public class GetDateBusinessDaysByStartDateHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetDateBusinessDaysByStartDateHandler(
            IDocumentDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDateBusinessDaysByStartDateRequest>(
                            nameof(GetDateBusinessDaysByStartDateRequest.IdSchool),
                            nameof(GetDateBusinessDaysByStartDateRequest.StartDate),
                            nameof(GetDateBusinessDaysByStartDateRequest.TotalDays),
                            nameof(GetDateBusinessDaysByStartDateRequest.CountHoliday));

            var result = await CountDateBusinessDaysByStartDate(new GetDateBusinessDaysByStartDateRequest
            {
                IdSchool = param.IdSchool,
                StartDate = param.StartDate,
                TotalDays = param.TotalDays,
                CountHoliday = param.CountHoliday
            });

            return Request.CreateApiResult2(result as object);
        }

        public int CountTotalWeekendBetweenDates(DateTime startDate, DateTime endDate)
        {
            var totalDays = (endDate - startDate).Days;

            int totalWeekendDays = 0;

            for (int i = 0; i <= totalDays; i++)
            {
                if (startDate.AddDays(i).DayOfWeek == DayOfWeek.Saturday)
                    totalWeekendDays++;
                if (startDate.AddDays(i).DayOfWeek == DayOfWeek.Sunday)
                    totalWeekendDays++;
            }

            return totalWeekendDays;
        }

        public async Task<GetDateBusinessDaysByStartDateResult> CountDateBusinessDaysByStartDate(GetDateBusinessDaysByStartDateRequest param)
        {
            var result = new GetDateBusinessDaysByStartDateResult();
            var date = param.StartDate;
            var days = param.TotalDays;

            if (param.TotalDays < 0)
            {
                throw new ArgumentException("Days cannot be negative", "days");
            }

            if (param.TotalDays == 0)
            {
                result.EndDate = param.StartDate;
                return result;
            }

            if (param.StartDate.DayOfWeek == DayOfWeek.Saturday)
            {
                date = date.AddDays(2);
                days -= 1;
            }
            else if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
                days -= 1;
            }

            date = date.AddDays(days / 5 * 7);
            int extraDays = days % 5;

            if ((int)date.DayOfWeek + extraDays > 5)
            {
                extraDays += 2;
            }

            // holiday counter
            var tempResultEndDateBeforeHoliday = date.AddDays(extraDays);
            var tempResultEndDateAfterHoliday = tempResultEndDateBeforeHoliday;
            int countTotalDaysHoliday = 0;
            int extraHolidayDays = 0;
            if (param.CountHoliday)
            {
                var getTotalHolidayBetweenTwoDatesList = await _dbContext.Entity<TrEventDetail>()
                                                        .Include(x => x.Event)
                                                        .Where(x =>
                                                                    param.StartDate.Date <= x.EndDate.Date &&
                                                                    x.Event.AcademicYear.IdSchool == param.IdSchool &&
                                                                    (
                                                                        x.Event.EventType.Code.ToUpper().Contains("PUBLIC HOLIDAY") ||
                                                                        x.Event.EventType.Description.ToUpper().Contains("PUBLIC HOLIDAY")
                                                                    ) &&
                                                                    x.Event.StatusEvent == "Approved")
                                                        .Select(x => new GetDateBusinessDaysByStartDateRequest_DateRange
                                                        {
                                                            StartDate = x.StartDate,
                                                            EndDate = x.EndDate,
                                                            TotalDays = (x.EndDate - x.StartDate).Days
                                                        })
                                                        .ToListAsync(CancellationToken);

                tempResultEndDateAfterHoliday = CalculateEndDateRecursive(param.StartDate, param.TotalDays, getTotalHolidayBetweenTwoDatesList);
            }

            result.EndDate = param.CountHoliday ? tempResultEndDateAfterHoliday : tempResultEndDateBeforeHoliday;

            return result;
        }

        private DateTime CalculateEndDateRecursive(DateTime newDate, int processDays, List<GetDateBusinessDaysByStartDateRequest_DateRange> holidayList)
        {
            var currDateIsHoliday = holidayList
                                        .Where(x => newDate.Date >= x.StartDate.Date &&
                                                    newDate.Date <= x.EndDate.Date)
                                        .Any();


            if (processDays <= 0 && !currDateIsHoliday && newDate.DayOfWeek != DayOfWeek.Saturday && newDate.DayOfWeek != DayOfWeek.Sunday)
                return newDate;

            if (!currDateIsHoliday && newDate.DayOfWeek != DayOfWeek.Saturday && newDate.DayOfWeek != DayOfWeek.Sunday)
                processDays -= 1;

            newDate = newDate.AddDays(1);
            return CalculateEndDateRecursive(newDate, processDays, holidayList);
        }
    }
}
