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
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetHistoryInvolvementByStudentHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetHistoryInvolvementByStudentRequest.IdStudent),
        };
        private static readonly string[] _columns = { "StudentName", "EventName", "Activity", "Award", "StartDate", "EndDate", "ApprovalStatus" };
        private readonly ISchedulingDbContext _dbContext;

        public GetHistoryInvolvementByStudentHandler(ISchedulingDbContext SchoolEventDbContext)
        {
            _dbContext = SchoolEventDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSchoolEventInvolvementRequest>(_requiredParams);
            var predicate = PredicateBuilder.Create<TrEvent>(x => x.IsActive==true);


            if (!string.IsNullOrEmpty(param.StartDate))
                predicate = predicate.And(x => x.EventDetails.Any(e => e.StartDate >= Convert.ToDateTime(param.StartDate) && e.EndDate >= Convert.ToDateTime(param.StartDate)));
            if (!string.IsNullOrEmpty(param.EndDate))
                predicate = predicate.And(x => x.EventDetails.Any(e => e.StartDate <= Convert.ToDateTime(param.EndDate) && e.EndDate <= Convert.ToDateTime(param.EndDate)));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Name, param.SearchPattern()));

            // var query = _dbContext.Entity<TrEvent>()
            //     .Include(e => e.EventCoordinators)
            //     .Include(e => e.EventType)
            //     .Include(e => e.EventApprovers)
            //     .Include(e => e.EventActivities).ThenInclude(e => e.EventActivityPICs)
            //     .Include(e => e.EventActivities).ThenInclude(e => e.EventActivityRegistrants)
            //     .Include(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForAttendanceStudents).ThenInclude(e => e.EventIntendedForAttendancePICStudents)
            //     .Include(x => x.EventDetails)
            //     .Include(e => e.EventApprovals)
            //     .Where(predicate)
            //    .Select(x => new
            //    {
            //        EventId = x.Id,
            //        EventName = x.Name,
            //        IsAssignedAsCoordinator = x.EventCoordinators.Any(e => e.IdEvent == x.Id && e.IdUser == param.IdUser),
            //        IsAssignedAsActivityPIC = x.EventActivities.Any(y => y.EventActivityPICs.Any(z => z.IdUser == param.IdUser)),
            //        IsAssignedAsRegistrant = x.EventActivities.Any(y => y.EventActivityRegistrants.Any(z => z.IdUser == param.IdUser)),
            //        IsAssignedAsApproval = x.EventApprovers.Any(e => e.IdEvent == x.Id && e.IdUser == param.IdUser),
            //        IsAssignedAsAttendentPIC = x.EventIntendedFor.Any(e => e.EventIntendedForAttendanceStudents.Any(e => e.EventIntendedForAttendancePICStudents.Any(e => e.IdUser == param.IdUser))),
            //        IsAttendentEventCreator = x.UserIn == param.IdUser,
            //        StartDate = x.EventDetails.Min(e=>e.StartDate),
            //        EndDate = x.EventDetails.Max(e => e.EndDate),
            //        ApprovalSatatus = x.StatusEvent,
            //        ApprovalDescription = x.DescriptionEvent,
            //        SortApprovalSatatus = x.StatusEvent.Contains("On Review") == true ? 0 : 1,
            //        CanDelete = x.StatusEvent == "Approved" ?
            //                     (x.EventApprovals.Where(e => e.IdEvent == x.Id && e.State == x.EventApprovals.Select(e => e.State).Max()).Count() > 0 ? true : false)
            //                     :x.StatusEvent == "Decline" ? true : false,
            //        CanEdit = x.StatusEvent.Contains("On Review") == true ? false : true,
            //        EventType = x.EventType.Description
            //    })
            //    .Where(e => e.IsAssignedAsCoordinator || e.IsAssignedAsActivityPIC || e.IsAssignedAsRegistrant|| e.IsAssignedAsApproval|| e.IsAssignedAsAttendentPIC || e.IsAttendentEventCreator)
            //    .OrderBy(x => x.SortApprovalSatatus);

            // //ordering
            // switch (param.OrderBy)
            // {
            //     case "EventName":
            //         query = param.OrderType == OrderType.Desc
            //             ? query.OrderByDescending(x => x.EventName)
            //             : query.OrderBy(x => x.EventName);
            //         break;
            //     case "EventType":
            //         query = param.OrderType == OrderType.Desc
            //             ? query.OrderByDescending(x => x.EventType)
            //             : query.OrderBy(x => x.EventType);
            //         break;
            //     case "StartDate":
            //         query = param.OrderType == OrderType.Desc
            //             ? query.OrderByDescending(x => x.StartDate)
            //             : query.OrderBy(x => x.StartDate);
            //         break;
            //     case "EndDate":
            //         query = param.OrderType == OrderType.Desc
            //             ? query.OrderByDescending(x => x.EndDate)
            //             : query.OrderBy(x => x.EndDate);
            //         break;
            //     case "ApprovalStatus":
            //         query = param.OrderType == OrderType.Desc
            //             ? query.OrderByDescending(x => x.ApprovalSatatus)
            //             : query.OrderBy(x => x.ApprovalSatatus);
            //         break;
            //     case "ApprovalDescription":
            //         query = param.OrderType == OrderType.Desc
            //             ? query.OrderByDescending(x => x.ApprovalDescription)
            //             : query.OrderBy(x => x.ApprovalDescription);
            //         break;
            // };

            var query = _dbContext.Entity<TrEvent>()
                .Include(X => X.EventDetails);


            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = (IReadOnlyList<IItemValueVm>)result.Select(x => new GetHistoryInvolvementByStudentResult
                {
                    Grades = new CodeWithIdVm
                    {
                        Id = "idgrade",
                        Code =  "EL",
                        Description = "Elementary School",
                    },
                    AcademicYear = new CodeWithIdVm
                    {
                        Id = "BEKASI2021",
                        Code =  "2021",
                        Description = "2021-2022",
                    },
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);
                items = result.Select(x => new GetSchoolEventInvolvementResult
                {
                    EventId = x.Id,
                    StudentId = "123456",
                    StudentName = "Nanda Aditya",
                    EventName = x.Name,
                    ActivityName = "Spelling Bee Competition Day1",
                    AwardName = "Participant",
                    StartDate = x.EventDetails.First().StartDate,
                    EndDate = x.EventDetails.First().EndDate,
                    ApprovalSatatus = x.StatusEventAward,
                    CanApprove = x.StatusEventAward != "Approved" ? true : false
                }).ToList();
            }


            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
