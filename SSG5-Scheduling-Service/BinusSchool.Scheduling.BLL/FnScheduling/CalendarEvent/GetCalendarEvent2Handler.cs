//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using BinusSchool.Common.Comparers;
//using BinusSchool.Common.Constants;
//using BinusSchool.Common.Extensions;
//using BinusSchool.Common.Functions.Handler;
//using BinusSchool.Common.Model;
//using BinusSchool.Common.Model.Abstractions;
//using BinusSchool.Common.Model.Enums;
//using BinusSchool.Common.Utils;
//using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
//using BinusSchool.Persistence.SchedulingDb.Abstractions;
//using BinusSchool.Persistence.SchedulingDb.Entities;
//using Microsoft.EntityFrameworkCore;

//namespace BinusSchool.Scheduling.FnSchedule.CalendarEvent
//{
//    public class GetCalendarEvent2Handler : FunctionsHttpSingleHandler
//    {
//        private readonly ISchedulingDbContext _dbContext;

//        public GetCalendarEvent2Handler(ISchedulingDbContext dbContext)
//        {
//            _dbContext = dbContext;
//        }

//        protected override async Task<ApiErrorResult<object>> Handler()
//        {
//            var param = Request.ValidateParams<GetCalendarEvent2Request>(nameof(GetCalendarEvent2Request.IdSchool),
//                nameof(GetCalendarEvent2Request.StartDate), nameof(GetCalendarEvent2Request.EndDate));

//            // convert param date to start until end day
//            param.StartDate = new DateTime(param.StartDate.Year, param.StartDate.Month, param.StartDate.Day, 0, 0, 0);
//            param.EndDate = new DateTime(param.EndDate.Year, param.EndDate.Month, param.EndDate.Day, 23, 59, 59);

//            var (items, count, columns) = await GetCalendarEvents(param);
//            var props = param.CreatePaginationProperty(count).AddColumnProperty(columns);

//            return Request.CreateApiResult2(items as object, props);
//        }

//        #region Public Method

//        public async Task<(IReadOnlyList<IItemValueVm> items, int count, IEnumerable<string> columns)> GetCalendarEvents(GetCalendarEvent2Request param)
//        {
//            var columns = new[] { "name" };

//            // var predicate = PredicateBuilder.Create<MsEvent>(x => param.IdSchool.Any(y => y == x.EventType.AcademicYear.IdSchool));
//            var predicate = PredicateBuilder.Create<MsEvent>(x => true);
//            predicate = predicate.And(x => x.EventDetails.Any(y
//                => y.StartDate == param.StartDate || y.EndDate == param.EndDate
//                || (y.StartDate < param.StartDate
//                    ? (y.EndDate > param.StartDate && y.EndDate < param.EndDate) || y.EndDate > param.EndDate
//                    : (param.EndDate > y.StartDate && param.EndDate < y.EndDate) || param.EndDate > y.EndDate)));

//            if (param.Ids?.Any() ?? false)
//                predicate = predicate.And(x => param.Ids.Contains(x.Id));
//            if (!string.IsNullOrWhiteSpace(param.Search))
//                predicate = predicate.And(x => EF.Functions.Like(x.Name, param.SearchPattern()));
//            if (!string.IsNullOrEmpty(param.IdEventType))
//                predicate = predicate.And(x => x.IdEventType == param.IdEventType);

//            // chained filter: grade > level > acadyear
//            if (!string.IsNullOrEmpty(param.IdGrade))
//                predicate = predicate.And(x
//                    => x.EventIntendedFor.EventIntendedForGrades.Any(y => y.IdGrade == param.IdGrade)
//                    || x.EventIntendedFor.EventIntendedForGradeSubjects.Any(y => y.IdGrade == param.IdGrade)
//                    || x.EventIntendedFor.EventIntendedForSubjectStudents.Any(y => y.Subject.IdGrade == param.IdGrade)
//                    || x.EventIntendedFor.EventIntendedForGradeStudents.Any(y => y.Homeroom.IdGrade == param.IdGrade));
//            else if (!string.IsNullOrEmpty(param.IdLevel))
//                predicate = predicate.And(x
//                    => x.EventIntendedFor.EventIntendedForGrades.Any(y => y.Grade.IdLevel == param.IdLevel)
//                    || x.EventIntendedFor.EventIntendedForGradeSubjects.Any(y => y.Grade.IdLevel == param.IdLevel)
//                    || x.EventIntendedFor.EventIntendedForSubjectStudents.Any(y => y.Subject.Grade.IdLevel == param.IdLevel)
//                    || x.EventIntendedFor.EventIntendedForGradeStudents.Any(y => y.Homeroom.Grade.IdLevel == param.IdLevel)
//                    || x.EventIntendedFor.IntendedFor == "ALL");
//            else if (!string.IsNullOrEmpty(param.IdAcadyear))
//                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcadyear);

//            if (!string.IsNullOrEmpty(param.Role))
//                predicate = predicate.And(x => x.EventIntendedFor.IntendedFor == "ALL" || x.EventIntendedFor.IntendedFor == param.Role);
//            if (!string.IsNullOrEmpty(param.IdUser))
//                predicate = predicate.And(x => x.EventDetails.Any(y => y.UserEvents.Any(z => z.IdUser == param.IdUser)));
//            if (param.ExcludeHiddenEvent.HasValue && param.ExcludeHiddenEvent.Value)
//                predicate = predicate.And(x => x.IsShowOnCalendarAcademic);

//            var query = _dbContext.Entity<MsEvent>().Where(predicate);
//            query = param.OrderBy switch
//            {
//                "name" => query.OrderByDynamic(param),
//                _ => query.OrderBy(x => x.EventDetails.Min(y => y.StartDate))
//            };

//            IReadOnlyList<IItemValueVm> items = default;
//            if (param.Return == CollectionType.Lov)
//            {
//                items = await query
//                    .Select(x => new ItemValueVm(x.Id, x.Name))
//                    .ToListAsync(CancellationToken);
//            }
//            else
//            {
//                items = await query
//                    .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForGradeSubjects).ThenInclude(x => x.EventIntendedForSubjects).ThenInclude(x => x.Subject)
//                    .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForGradeSubjects).ThenInclude(x => x.Grade)
//                    .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForDepartments).ThenInclude(x => x.Department)
//                    .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForGrades).ThenInclude(x => x.Grade)
//                    .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForPersonalStudents).ThenInclude(x => x.Student)
//                    .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForGradeStudents).ThenInclude(x => x.Homeroom).ThenInclude(x => x.Grade)
//                    .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForSubjectStudents).ThenInclude(x => x.Subject)
//                    .SetPagination(param)
//                    .Select(x => new GetCalendarEvent2Result
//                    {
//                        Id = x.Id,
//                        Name = x.Name,
//                        Dates = x.EventDetails.Select(y => new DateTimeRange
//                        {
//                            Start = y.StartDate,
//                            End = y.EndDate
//                        }),
//                        // EventType = new CalendarEventTypeVm
//                        // {
//                        //     Id = x.IdEventType,
//                        //     Code = x.EventType.Code,
//                        //     Description = x.EventType.Description,
//                        //     Color = x.EventType.Color
//                        // },
//                        Role = x.EventIntendedFor.IntendedFor,
//                        Option = x.EventIntendedFor.Option,
//                        AttendanceOption = x.EventIntendedFor.IntendedFor == RoleConstant.Student
//                            ? x.EventIntendedFor.EventIntendedForAttendanceStudents.First().Type
//                            : EventIntendedForAttendanceStudent.No,
//                        Description = (param.ExcludeOptionMetadata.HasValue && param.ExcludeOptionMetadata.Value)
//                            ? null
//                            : GetEventDescription(x.EventIntendedFor),
//                        LastUpdate = x.DateUp ?? x.DateIn ?? DateTime.MinValue
//                    })
//                    .ToListAsync(CancellationToken);
//            }

//            var count = param.CanCountWithoutFetchDb(items.Count)
//                ? items.Count
//                : await query.Select(x => x.Id).CountAsync(CancellationToken);

//            return (items, count, columns);
//        }

//        #endregion

//        #region Private Method

//        private static string GetEventDescription(MsEventIntendedFor intendedFor)
//        {
//            var desc = intendedFor.IntendedFor switch
//            {
//                RoleConstant.Teacher => intendedFor.Option switch
//                {
//                    EventOptionType.Subject => string.Join(Environment.NewLine, intendedFor.EventIntendedForGradeSubjects
//                        .Select(x => $"{x.Grade.Description} {string.Join(", ", x.EventIntendedForSubjects.Select(y => y.Subject.Description))}")),
//                    EventOptionType.Department => string.Join(", ", intendedFor.EventIntendedForDepartments.Select(x => x.Department.Description)),
//                    EventOptionType.Grade => string.Join(", ", intendedFor.EventIntendedForGrades.Select(x => x.Grade.Description).Distinct()),
//                    _ => null
//                },
//                RoleConstant.Student => intendedFor.Option switch
//                {
//                    EventOptionType.Personal => string.Join(", ", intendedFor.EventIntendedForPersonalStudents
//                        .Select(x => NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName))),
//                    EventOptionType.Grade => string.Join(", ", intendedFor.EventIntendedForGradeStudents.Select(x => x.Homeroom.Grade.Description).Distinct()),
//                    EventOptionType.Subject => string.Join(", ", intendedFor.EventIntendedForSubjectStudents.Select(x => x.Subject.Description)),
//                    _ => null
//                },
//                _ => null
//            };

//            return desc;
//        }

//        #endregion
//    }
//}
