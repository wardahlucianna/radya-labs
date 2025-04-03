using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Comparers;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetEventCalenderHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetEventCalenderHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetEventCalendarRequest>(nameof(GetEventCalendarRequest.IdSchool),
                nameof(GetEventCalendarRequest.StartDate), nameof(GetEventCalendarRequest.EndDate));

            // convert param date to start until end day
            param.StartDate = new DateTime(param.StartDate.Year, param.StartDate.Month, param.StartDate.Day, 0, 0, 0);
            param.EndDate = new DateTime(param.EndDate.Year, param.EndDate.Month, param.EndDate.Day, 23, 59, 59);

            var (items, count, columns) = await GetCalendarEvents(param);
            var props = param.CreatePaginationProperty(count).AddColumnProperty(columns);

            return Request.CreateApiResult2(items as object, props);
        }

        #region Public Method

        public async Task<(IReadOnlyList<IItemValueVm> items, int count, IEnumerable<string> columns)> GetCalendarEvents(GetEventCalendarRequest param)
        {
            var columns = new[] { "name" };

            var predicate = PredicateBuilder.Create<TrEvent>(x => param.IdSchool.Any(y => y == x.EventType.AcademicYear.IdSchool));
            predicate = predicate.And(x => x.IsShowOnCalendarAcademic == true && x.StatusEvent == "Approved" && x.EventDetails.Any(y
                => y.StartDate == param.StartDate || y.EndDate == param.EndDate
                || (y.StartDate < param.StartDate
                    ? (y.EndDate > param.StartDate && y.EndDate < param.EndDate) || y.EndDate > param.EndDate
                    : (param.EndDate > y.StartDate && param.EndDate < y.EndDate) || param.EndDate > y.EndDate)));

            if (param.Ids?.Any() ?? false)
                predicate = predicate.And(x => param.Ids.Contains(x.Id));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x => EF.Functions.Like(x.Name, param.SearchPattern()));

            if (param.IdEventTypes != null)
                predicate = predicate.And(x => param.IdEventTypes.Contains(x.IdEventType));

            var query = _dbContext.Entity<TrEvent>()
                    .Include(x => x.EventType)
                    .Include(x => x.EventDetails)
                    .Include(x => x.EventIntendedFor).ThenInclude(e=>e.EventIntendedForDepartments).ThenInclude(e=>e.Department)
                    .Include(x => x.EventIntendedFor).ThenInclude(e=>e.EventIntendedForGradeStudents).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                    .Include(x => x.EventIntendedFor).ThenInclude(e=>e.EventIntendedForLevelStudents).ThenInclude(e => e.Level)
                    .Include(x => x.EventIntendedFor).ThenInclude(e=>e.EventIntendedForPositions).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.Position)
                    .Where(predicate)
                    .OrderBy(x => x.DateIn);

            IReadOnlyList<IItemValueVm> items = default;

            items = await query
                    .SetPagination(param)
                    .Select(x => new GetEventCalendarResult
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Dates = x.EventDetails != null ? x.EventDetails.Select(y => new DateTimeRange
                        {
                            Start = y.StartDate,
                            End = y.EndDate
                        }).ToList() : null,
                        EventType = new CalendarEventTypeVm
                        {
                            Id = x.EventType.Id,
                            Code = x.EventType.Code,
                            Description = x.EventType.Description,
                            Color = x.EventType.Color
                        },
                        IntendedFor = x.EventIntendedFor != null ? x.EventIntendedFor.Select(y => new IntendedFor
                        {
                            Role = y.IntendedFor,
                            Option = y.Option,
                            Detail = GetDetailIntendedFor(y)
                        }).ToList() : null,
                        Description = null,
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return (items, count, columns);
        }

        public static List<string> GetDetailIntendedFor(TrEventIntendedFor IntendedFor)
        {
            List<string> Items = new List<string>();

            if (IntendedFor.Option == "Department")
            {
                var GetDepartemen = IntendedFor.EventIntendedForDepartments.Select(e => e.Department.Description).Distinct().ToList();
                var Departemen = "";
                foreach (var ItemDepartemen in GetDepartemen)
                {
                    Departemen += GetDepartemen.IndexOf(ItemDepartemen) == 0 ? ItemDepartemen : ", " + ItemDepartemen;
                }

                Items.Add(Departemen);
            }
            else if (IntendedFor.Option == "Position")
            {
                var GetPosition = IntendedFor.EventIntendedForPositions.Select(e => e.TeacherPosition.Position.Description).Distinct().ToList();
                var Position = "";
                foreach (var ItemPosition in GetPosition)
                {
                    Position += GetPosition.IndexOf(ItemPosition) == 0 ? ItemPosition : ", " + ItemPosition;
                }

                Items.Add(Position);
            }
            else if (IntendedFor.Option == "Level")
            {
                var GetLevel = IntendedFor.EventIntendedForLevelStudents.Select(e => e.Level.Description).Distinct().ToList();
                var Level = "";
                foreach (var ItemLevel in GetLevel)
                {
                    Level += GetLevel.IndexOf(ItemLevel) == 0 ? ItemLevel : ", " + ItemLevel;
                }

                Items.Add(Level);
            }
            else if (IntendedFor.Option == "Grade")
            {
                var GetLevel = IntendedFor.EventIntendedForGradeStudents.Select(e => e.Homeroom.Grade.Level).Distinct().ToList();

                foreach (var ItemLevel in GetLevel)
                {
                    var GetGrade = ItemLevel.MsGrades.ToList();
                    var Grade = "";
                    foreach (var ItemGrade in GetGrade)
                    {
                        Grade += GetGrade.IndexOf(ItemGrade) == 0 ? ItemGrade.Description : ", " + ItemGrade.Description;
                    }

                    Items.Add($"{ItemLevel.Description}: {Grade}");
                }
            }

            return (Items);
        }

        #endregion
    }
}
