using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Abstractions;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetSchoolEventSummaryHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetSchoolEventSummaryRequest.IntendedFor),
        };
        private static readonly string[] _columns = { "ParticipantName", "BinusianId", "Level", "Grade", "Homeroom", "EventName", "Place", "Activity", "Award", "StartDate", "EndDate", "ApprovalStatus" };
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetSchoolEventSummaryHandler(
            ISchedulingDbContext SchoolEventDbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = SchoolEventDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSchoolEventSummaryRequest>(_requiredParams);

            var CheckRole = await (from a in _dbContext.Entity<MsUser>()
                                   join r in _dbContext.Entity<MsUserRole>() on a.Id equals r.IdUser
                                   join rg in _dbContext.Entity<LtRole>() on r.IdRole equals rg.Id
                                   where a.Id == param.IdUser

                                   select new LtRole
                                   {
                                       IdRoleGroup = rg.IdRoleGroup,
                                       IdSchool = rg.IdSchool
                                   }).FirstOrDefaultAsync(CancellationToken);

            if (CheckRole == null)
                throw new BadRequestException($"User in this role not found");

            // var currentAY = await _dbContext.Entity<MsPeriod>()
            //    .Include(x => x.Grade)
            //        .ThenInclude(x => x.Level)
            //            .ThenInclude(x => x.AcademicYear)
            //    .Where(x => x.Grade.Level.AcademicYear.IdSchool == CheckRole.IdSchool)
            //    .Where(x => _dateTime.ServerTime.Date >= x.StartDate.Date)
            //    .Where(x => _dateTime.ServerTime.Date <= x.EndDate.Date)
            //    .Select(x => new
            //    {
            //        Id = x.Grade.Level.AcademicYear.Id
            //    }).FirstOrDefaultAsync();
           
            if (param.IntendedFor == "STUDENT")
            {
                var predicate = PredicateBuilder.Create<TrEventActivityAward>(x => param.IdSchool.Any(y => y == x.EventActivity.Event.EventType.AcademicYear.IdSchool));
                
                // if(currentAY != null)
                //     predicate = predicate.And(x => x.EventActivity.Event.IdAcademicYear == currentAY.Id);

                if(param.StartDate != null || param.EndDate != null)
                {
                    predicate = predicate.And(x => x.EventActivity.Event.IsStudentInvolvement == false && x.EventActivity.Event.StatusEvent == "Approved" && x.EventActivity.Event.EventDetails.Any(y
                        => y.StartDate == param.StartDate || y.EndDate == param.EndDate
                        || (y.StartDate < param.StartDate
                            ? (y.EndDate > param.StartDate && y.EndDate < param.EndDate) || y.EndDate > param.EndDate
                            : (param.EndDate > y.StartDate && param.EndDate < y.EndDate) || param.EndDate > y.EndDate)));
                
                }

                if (!string.IsNullOrWhiteSpace(param.Search))
                    predicate = predicate.And(x => EF.Functions.Like(x.EventActivity.Event.Name.ToUpper(), $"%{param.Search.ToUpper()}%")
                                            || EF.Functions.Like(x.HomeroomStudent.Student.Id, param.Search)
                                            || EF.Functions.Like(x.EventActivity.Activity.Description.ToUpper(), $"%{param.Search.ToUpper()}%")
                                            || EF.Functions.Like(x.Award.Description.ToUpper(), $"%{param.Search.ToUpper()}%")
                                            || EF.Functions.Like(x.HomeroomStudent.Student.FirstName.ToUpper(), $"%{param.Search.ToUpper()}%")
                                            );

                var query = _dbContext.Entity<TrEventActivityAward>()
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.Event)
                            .ThenInclude(x => x.EventDetails)
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.EventActivityPICs)
                            .ThenInclude(x => x.User)
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.EventActivityRegistrants)
                            .ThenInclude(x => x.User)
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.Activity)
                    .Include(x => x.HomeroomStudent)
                        .ThenInclude(x => x.Student)
                    .Include(x => x.HomeroomStudent)
                        .ThenInclude(x => x.Homeroom)
                            .ThenInclude(x => x.Grade)
                                .ThenInclude(x => x.Level)
                    .Include(x => x.Award)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(param.IdEvent))
                    query = query.Where(x => x.EventActivity.IdEvent == param.IdEvent);

                if (!string.IsNullOrEmpty(param.IdActivity))
                    query = query.Where(x => x.EventActivity.IdActivity == param.IdActivity);

                if (!string.IsNullOrEmpty(param.IdAward))
                    query = query.Where(x => x.IdAward == param.IdAward);

                switch (param.OrderBy)
                {
                    case "EventName":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.EventActivity.Event.Name)
                            : query.OrderBy(x => x.EventActivity.Event.Name);
                        break;
                    case "StudentName":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.HomeroomStudent.Student.FirstName)
                            : query.OrderBy(x => x.HomeroomStudent.Student.FirstName);
                        break;
                    case "Activity":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.EventActivity.Activity.Description)
                            : query.OrderBy(x => x.EventActivity.Activity.Description);
                        break;
                    case "Award":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.Award.Description)
                            : query.OrderBy(x => x.Award.Description);
                        break;
                    default:
                        query = param.OrderType == OrderType.Desc
                                                 ? query.OrderByDescending(x => x.EventActivity.Event.Name)
                                                 : query.OrderBy(x => x.EventActivity.Event.Name);
                        break;
                }
                var data = await query
                    .Where(predicate)
                    .SetPagination(param)
                    .Select(x => new GetSchoolEventSummary2Result
                    {
                        Activity = x.EventActivity.Activity.Description,
                        BinusianID = x.HomeroomStudent.Student.Id,
                        EventDates = x.EventActivity.Event.EventDetails.Select(x => new EventDate
                        {
                            StartDate = x.StartDate,
                            EndDate = x.EndDate,
                        }).ToList(),
                        EventName = x.EventActivity.Event.Name,
                        Grade = x.HomeroomStudent.Homeroom.Grade.Description,
                        Homeroom = x.HomeroomStudent.Homeroom.Grade.Code + x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                        Involvement = x.Award.Description,
                        IdEventActivityAward = x.Id,
                        Level = x.HomeroomStudent.Homeroom.Grade.Level.Code,
                        ParticipantName = x.HomeroomStudent.Student.MiddleName != null ? x.HomeroomStudent.Student.FirstName.Trim() + " " + x.HomeroomStudent.Student.MiddleName.Trim() + x.HomeroomStudent.Student.LastName.Trim() : x.HomeroomStudent.Student.FirstName.Trim() + " " + x.HomeroomStudent.Student.LastName.Trim(),
                        PIC = x.EventActivity.EventActivityPICs.Select(x => new User
                        {
                            IdBinusian = x.User.Id,
                            Name = x.User.DisplayName
                        }).ToList(),
                        Registratior = x.EventActivity.EventActivityRegistrants.Select(x => new User
                        {
                            IdBinusian = x.User.Id,
                            Name = x.User.DisplayName
                        }).ToList(),
                        Place = x.EventActivity.Event.Place
                    })
                    .ToListAsync();
                var count = param.CanCountWithoutFetchDb(data.Count)
              ? data.Count
              : await query.Select(x => x.Id).CountAsync(CancellationToken);
                return Request.CreateApiResult2(data as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
            }
            else
            {
                var predicate = PredicateBuilder.Create<TrEventActivityAwardTeacher>(x => param.IdSchool.Any(y => y == x.EventActivity.Event.EventType.AcademicYear.IdSchool));
                
                // if(currentAY != null)
                //     predicate = predicate.And(x => x.EventActivity.Event.IdAcademicYear == currentAY.Id);

                if(param.StartDate != null || param.EndDate != null)
                {
                    predicate = predicate.And(x => x.EventActivity.Event.IsStudentInvolvement == false && x.EventActivity.Event.StatusEvent == "Approved" && x.EventActivity.Event.EventDetails.Any(y
                        => y.StartDate == param.StartDate || y.EndDate == param.EndDate
                        || (y.StartDate < param.StartDate
                            ? (y.EndDate > param.StartDate && y.EndDate < param.EndDate) || y.EndDate > param.EndDate
                            : (param.EndDate > y.StartDate && param.EndDate < y.EndDate) || param.EndDate > y.EndDate)));
                }

                if (!string.IsNullOrWhiteSpace(param.Search))
                    predicate = predicate.And(x => EF.Functions.Like(x.EventActivity.Event.Name.ToUpper(), $"%{param.Search.ToUpper()}%")
                                            || EF.Functions.Like(x.Staff.IdBinusian, param.Search)
                                            || EF.Functions.Like(x.EventActivity.Activity.Description.ToUpper(), $"%{param.Search.ToUpper()}%")
                                            || EF.Functions.Like(x.Award.Description.ToUpper(), $"%{param.Search.ToUpper()}%")
                                            || EF.Functions.Like(x.Staff.FirstName.ToUpper(), $"%{param.Search.ToUpper()}%")
                                            );


                var query = _dbContext.Entity<TrEventActivityAwardTeacher>()
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.Event)
                            .ThenInclude(x => x.EventDetails)
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.EventActivityPICs)
                            .ThenInclude(x => x.User)
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.EventActivityRegistrants)
                            .ThenInclude(x => x.User)
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.Activity)
                    .Include(x => x.Award)
                    .AsQueryable();
                if (!string.IsNullOrEmpty(param.IdEvent))
                    query = query.Where(x => x.EventActivity.IdEvent == param.IdEvent);

                if (!string.IsNullOrEmpty(param.IdActivity))
                    query = query.Where(x => x.EventActivity.IdActivity == param.IdActivity);

                if (!string.IsNullOrEmpty(param.IdAward))
                    query = query.Where(x => x.IdAward == param.IdAward);

                switch (param.OrderBy)
                {
                    case "EventName":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.EventActivity.Event.Name)
                            : query.OrderBy(x => x.EventActivity.Event.Name);
                        break;
                    case "StudentName":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.Staff.FirstName)
                            : query.OrderBy(x => x.Staff.FirstName);
                        break;
                    case "Activity":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.EventActivity.Activity.Description)
                            : query.OrderBy(x => x.EventActivity.Activity.Description);
                        break;
                    case "Award":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.Award.Description)
                            : query.OrderBy(x => x.Award.Description);
                        break;
                    default:
                        query = param.OrderType == OrderType.Desc
                                                 ? query.OrderByDescending(x => x.EventActivity.Event.Name)
                                                 : query.OrderBy(x => x.EventActivity.Event.Name);
                        break;
                }
                var data = await query
                    .Where(predicate)
                    .SetPagination(param)
                    .Select(x => new GetSchoolEventSummary2Result
                    {
                        Activity = x.EventActivity.Activity.Description,
                        BinusianID = x.Staff.IdBinusian,
                        EventDates = x.EventActivity.Event.EventDetails.Select(x => new EventDate
                        {
                            StartDate = x.StartDate,
                            EndDate = x.EndDate,
                        }).ToList(),
                        EventName = x.EventActivity.Event.Name,
                        Grade = null,
                        Homeroom = null,
                        Involvement = x.Award.Description,
                        IdEventActivityAward = x.Id,
                        Level = null,
                        ParticipantName = x.Staff.FirstName,
                        PIC = x.EventActivity.EventActivityPICs.Select(x => new User
                        {
                            IdBinusian = x.User.Id,
                            Name = x.User.DisplayName
                        }).ToList(),
                        Registratior = x.EventActivity.EventActivityRegistrants.Select(x => new User
                        {
                            IdBinusian = x.User.Id,
                            Name = x.User.DisplayName
                        }).ToList(),
                        Place = x.EventActivity.Event.Place
                    })
                    .ToListAsync();
                var count = param.CanCountWithoutFetchDb(data.Count)
              ? data.Count
              : await query.Select(x => x.Id).CountAsync(CancellationToken);
                return Request.CreateApiResult2(data as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
            }
        }
    }
}
