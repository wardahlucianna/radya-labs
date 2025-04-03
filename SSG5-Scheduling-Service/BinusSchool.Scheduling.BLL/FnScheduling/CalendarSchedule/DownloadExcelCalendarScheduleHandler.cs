using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule;
using BinusSchool.Data.Model.School.FnSchool.AcademicYear;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnSchedule.CalendarSchedule
{
    public class DownloadExcelCalendarScheduleHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(DownloadExcelCalendarScheduleRequest.IdSchool),
            nameof(DownloadExcelCalendarScheduleRequest.StartDate), nameof(DownloadExcelCalendarScheduleRequest.EndDate),
            nameof(DownloadExcelCalendarScheduleRequest.IdUser), nameof(DownloadExcelCalendarScheduleRequest.Role)
        });
        private static readonly IEnumerable<DayOfWeek> _days = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>();

        private DateTime _minSchedule, _maxSchedule;

        private readonly ISchedulingDbContext _dbContext;
        private readonly GetCalendarScheduleHandler _scheduleHandler;
        private readonly GetCalendarScheduleV2Handler _scheduleV2Handler;

        public DownloadExcelCalendarScheduleHandler(ISchedulingDbContext dbContext, GetCalendarScheduleHandler scheduleHandler, GetCalendarScheduleV2Handler scheduleV2Handler)
        {
            _dbContext = dbContext;
            _scheduleHandler = scheduleHandler;
            _scheduleV2Handler = scheduleV2Handler;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<DownloadExcelCalendarScheduleRequest>(_requiredParams.Value);

            _minSchedule = param.StartDate;
            _maxSchedule = param.EndDate;

            var (schedules, _) = await _scheduleHandler.GetCalendarSchedules(
                new GetCalendarScheduleRequest
                {
                    IdSchool = new[] { param.IdSchool },
                    IdAcadyear = param.IdAcadyear,
                    IdLevel = param.IdLevel,
                    IdGrade = param.IdGrade,
                    StartDate = param.StartDate,
                    EndDate = param.EndDate,
                    IdUser = param.IdUser,
                    Role = param.Role,
                    GetAll = true
                }, Localizer);

            var (schedulesV2, _) = await _scheduleV2Handler.GetCalendarSchedules(
              new GetCalendarScheduleV2Request
              {
                  IdSchool = new[] { param.IdSchool },
                  IdAcadyear = param.IdAcadyear,
                  IdLevel = param.IdLevel,
                  IdGrade = param.IdGrade,
                  StartDate = param.StartDate,
                  EndDate = param.EndDate,
                  IdUser = param.IdUser,
                  Role = param.Role,
                  GetAll = true
              }, Localizer);


            var allMonths = DateTimeUtil.ToEachMonth(param.StartDate, param.EndDate);

            var predicatePeriod = PredicateBuilder.Create<MsPeriod>(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool);
            if (!string.IsNullOrEmpty(param.IdAcadyear))
                predicatePeriod = predicatePeriod.And(x => x.Grade.Level.IdAcademicYear == param.IdAcadyear);
            else
                predicatePeriod = predicatePeriod.And(x
                    => x.StartDate == param.StartDate || x.EndDate == param.EndDate 
                    || (x.StartDate < param.StartDate
                        ? (x.EndDate > param.StartDate && x.EndDate < param.EndDate) || x.EndDate > param.EndDate
                        : (param.EndDate > x.StartDate && param.EndDate < x.EndDate) || param.EndDate > x.EndDate));
            
            var periods = await _dbContext.Entity<MsPeriod>()
                .Where(predicatePeriod)
                .Select(x => new
                {
                    x.Grade.Level.IdAcademicYear,
                    x.Grade.Level.AcademicYear.Code,
                    x.Grade.Level.AcademicYear.Description,
                    x.StartDate,
                    x.EndDate
                })
                .Distinct()
                .ToListAsync(CancellationToken);
            var acadyears = periods
                .GroupBy(x => x.IdAcademicYear)
                .Select(x => new GetAcadyearByRangeResult
                {
                    Id = x.Key,
                    Code = x.First().Code,
                    Description = x.First().Description,
                    StartDate = x.Min(y => y.StartDate),
                    EndDate = x.Max(y => y.EndDate)
                })
                .Where(x => DateTimeUtil.IsIntersect(x.StartDate, x.EndDate, param.StartDate, param.EndDate));

            var userDisplayName = await _dbContext.Entity<MsUser>()
                .Where(x => x.Id == param.IdUser)
                .Select(x => x.DisplayName)
                .FirstOrDefaultAsync(CancellationToken);

            #region Dummy Shedules

            // schedules = new[]
            // {
            //     new GetCalendarScheduleResult
            //     {
            //         Id = "1",
            //         Name = "Event 1",
            //         Start = new DateTime(2021, 08, 01, 07, 30, 0),
            //         End = new DateTime(2021, 08, 02, 09, 0, 0)
            //     },
            //     new GetCalendarScheduleResult
            //     {
            //         Id = "1a",
            //         Name = "Event 1a",
            //         Start = new DateTime(2021, 08, 01, 08, 30, 0),
            //         End = new DateTime(2021, 08, 02, 10, 0, 0)
            //     },
            //     new GetCalendarScheduleResult
            //     {
            //         Id = "2",
            //         Name = "Event 2",
            //         Start = new DateTime(2021, 08, 31, 10, 0, 0),
            //         End = new DateTime(2021, 09, 01, 14, 0, 0)
            //     },
            //     new GetCalendarScheduleResult
            //     {
            //         Id = "3",
            //         Name = "Event 3",
            //         Start = new DateTime(2021, 12, 31, 07, 0, 0),
            //         End = new DateTime(2022, 01, 05, 17, 0, 0)
            //     },
            // };

            // generate random schedule
            // var rand = new Random();
            // var users = new[] {"Bambang", "Pamungkas", "Satria", "Bagas", "Kara"};
            // var homerooms = new[] {"10A", "10B", "10C", "10D", "11A", "11B", "12C", "12D"};
            // schedules = Enumerable.Range(1, 10)
            //     .Select(x => new GetCalendarScheduleResult
            //     {
            //         Id = x.ToString(),
            //         Name = $"Event {x}",
            //         Start = new DateTime(2020, rand.Next(1, 12), rand.Next(1, 2), rand.Next(24), rand.Next(60), 0),
            //         End = new DateTime(2021, rand.Next(1, 12), rand.Next(1, 2), rand.Next(24), rand.Next(60), 0),
            //         Teacher = new NameValueVm(x.ToString(), users[rand.Next(users.Length)].ToString()),
            //         Homeroom = new ItemValueVm(x.ToString(), homerooms[rand.Next(homerooms.Length)].ToString()),
            //         EventType = new Data.Model.Scheduling.FnSchedule.CalendarEvent.CalendarEventTypeVm
            //         {
            //             Id = Guid.Empty.ToString()
            //         }
            //     })
            //     .OrderBy(x => x.Start);

            #endregion

            var excelSchedules = GenerateExcel(allMonths, schedules.ToList(), acadyears.ToArray(), new NameValueVm(param.IdUser, userDisplayName), param.Role);

            return new FileContentResult(excelSchedules, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"CalendarSchedule_{param.IdUser}_{DateTime.Now.Ticks}.xlsx"
            };
        }

        private byte[] GenerateExcel(IEnumerable<DateTime> months, List<GetCalendarScheduleResult> schedules, GetAcadyearByRangeResult[] acadyears, NameValueVm user, string role)
        {
            var workbook = new XSSFWorkbook();

            var fontBold = workbook.CreateFont();
            fontBold.IsBold = true;

            var boldStyle = workbook.CreateCellStyle();
            boldStyle.SetFont(fontBold);

            var borderCellStyle = workbook.CreateCellStyle();
            borderCellStyle.BorderBottom = BorderStyle.Thin;
            borderCellStyle.BorderLeft = BorderStyle.Thin;
            borderCellStyle.BorderRight = BorderStyle.Thin;
            borderCellStyle.BorderTop = BorderStyle.Thin;

            var headerCellStyle = workbook.CreateCellStyle();
            headerCellStyle.CloneStyleFrom(borderCellStyle);
            headerCellStyle.SetFont(fontBold);
                        
            // create sheet for each month
            foreach (var month in months)
            {
                var sheet = workbook.CreateSheet(month.ToString("MMMM yyyy"));

                var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
                var firstDayOfMonth = month.DayOfWeek;
                var weeksInMonth = (int)Math.Ceiling(((int)firstDayOfMonth + daysInMonth) / 7d);
                var startDayOfMonth = month;
                var endDayOfMonth = startDayOfMonth.Add(new TimeSpan(daysInMonth - 1, 23, 59, 59));
                var shouldRenderDate = false;
                var currentRow = 6;

                // select schedule per month
                var schedulesInMonth = schedules
                    .Where(x => DateTimeUtil.IsIntersect(x.Start, x.End, startDayOfMonth, endDayOfMonth))
                    .ToArray();
                
                // reduce filter in the next iteration by removing already fetched schedules
                foreach (var scheduleWillRemove in schedulesInMonth)
                    schedules.Remove(scheduleWillRemove);

                // Acadyear
                var rowAy = sheet.CreateRow(0);
                var cellAy = rowAy.CreateCell(0);
                cellAy.SetCellValue("Academic Year");
                cellAy.CellStyle = boldStyle;
                rowAy.CreateCell(1).SetCellValue(string.Join(", ", acadyears
                    .Where(x => DateTimeUtil.IsIntersect(x.StartDate, x.EndDate, startDayOfMonth, endDayOfMonth))
                    .Select(x => x.Description)));
                
                // Homeroom
                var rowHr = sheet.CreateRow(1);
                var cellHr = rowHr.CreateCell(0);
                cellHr.SetCellValue("Homeroom");
                cellHr.CellStyle = boldStyle;
                rowHr.CreateCell(1).SetCellValue(string.Join(", ", schedulesInMonth.Where(x => x.Homeroom != null).Select(x => x.Homeroom.Description).Distinct()));
                
                // Student/Teacher Name
                var rowUser = sheet.CreateRow(2);
                var cellUser = rowUser.CreateCell(0);
                cellUser.SetCellValue($"{role.ToLower().ToUpperFirst()} Name");
                cellUser.CellStyle = boldStyle;
                rowUser.CreateCell(1).SetCellValue(user.Name);
                
                // Schedule Start/End
                var rowDate = sheet.CreateRow(3);
                var cellDate = rowDate.CreateCell(0);
                cellDate.SetCellValue("Generate Date");
                cellDate.CellStyle = boldStyle;
                rowDate.CreateCell(1).SetCellValue(_minSchedule.ToString("dd-MM-yyyy"));
                var cellDate2 = rowDate.CreateCell(2);
                cellDate2.SetCellValue("Until");
                cellDate2.CellStyle = boldStyle;
                rowDate.CreateCell(3).SetCellValue(_maxSchedule.ToString("dd-MM-yyyy"));
                
                for (var i = 0; i < weeksInMonth; i++)
                {
                    var rowDay = sheet.CreateRow(currentRow);
                    var rowAdded = 0;

                    for (var j = 0; j < _days.Count(); j++)
                    {
                        // check first day of month with cureent day of week
                        // if match, than start render calendar
                        if (startDayOfMonth.Day == 1 && _days.ElementAt(j) == firstDayOfMonth)
                            shouldRenderDate = true;

                        // render date & schedule value here
                        if (shouldRenderDate)
                        {
                            // rendering day & date
                            var header = rowDay.CreateCell(j);
                            header.CellStyle = headerCellStyle;
                            header.SetCellValue($"{_days.ElementAt(j)} ({startDayOfMonth.Day})");

                            // only render requested range date of schedule
                            if (startDayOfMonth.Date >= _minSchedule.Date && startDayOfMonth.Date <= _maxSchedule.Date)
                            {
                                var schedulesInDay = schedulesInMonth
                                    .Where(x => DateTimeUtil.IsIntersect(x.Start, x.End, startDayOfMonth, startDayOfMonth.AddHours(24)))
                                    .OrderBy(x => x.Start.Hour)
                                    .ToArray();

                                for (var k = 0; k < schedulesInDay.Length; k++)
                                {
                                    // create new/use existing row to render schedule
                                    var rowSchedule = sheet.GetRow(currentRow + k + 1) ?? sheet.CreateRow(currentRow + k + 1);
                                    var cellValue = $"{schedulesInDay[k].Name} ({schedulesInDay[k].Start:HH:mm}-{schedulesInDay[k].End:HH:mm})";
                                    
                                    // event type with empty guid is class schedule
                                    if (schedulesInDay[k].EventType.Id == Guid.Empty.ToString())
                                        cellValue = $"{schedulesInDay[k].Homeroom.Description} - {schedulesInDay[k].Teacher.Name} - {cellValue}";

                                    rowSchedule.CreateCell(j).SetCellValue(cellValue);
                                }

                                // update row added
                                if (schedulesInDay.Length > rowAdded)
                                    rowAdded = schedulesInDay.Length;
                            }
                        }

                        // make sure render calendar not overflow with total days in month
                        if (shouldRenderDate && startDayOfMonth.Day < daysInMonth)
                            startDayOfMonth = startDayOfMonth.AddDays(1);
                        else
                            shouldRenderDate = false;
                    }

                    // apply bordered column style
                    for (var j = 0; j < _days.Count(); j++)
                    {
                        for (var k = 0; k < rowAdded; k++)
                        {
                            if (sheet.GetRow(currentRow).GetCell(j) != null)
                            {
                                var rowSchedule = sheet.GetRow(currentRow + k + 1) ?? sheet.CreateRow(currentRow + k + 1);
                                var cellSchedule = rowSchedule.GetCell(j) ?? rowSchedule.CreateCell(j);
                                cellSchedule.CellStyle = borderCellStyle;
                            }
                        }
                    }

                    currentRow += 3 + rowAdded;
                }

                // make auto size column
                for (var i = 0; i < _days.Count(); i++)
                {
                    sheet.AutoSizeColumn(i);
                }
            }

            using var ms = new MemoryStream();
            ms.Position = 0;
            workbook.Write(ms);

            return ms.ToArray();
        }
    }
}
