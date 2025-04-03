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
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetSchoolEventInvolvementHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetSchoolEventInvolvementRequest.IdUser),
        };
        private static readonly string[] _columns = { "StudentName", "EventName", "Activity", "Award", "StartDate", "EndDate", "ApprovalStatus" };
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetSchoolEventInvolvementHandler(
            ISchedulingDbContext SchoolEventDbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = SchoolEventDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSchoolEventInvolvementRequest>(_requiredParams);

            var CheckRole = await (from a in _dbContext.Entity<MsUser>()
                                   join r in _dbContext.Entity<MsUserRole>() on a.Id equals r.IdUser
                                   join rg in _dbContext.Entity<LtRole>() on r.IdRole equals rg.Id
                                   where a.Id == param.IdUser
                                    
                                  select new LtRole
                                  {
                                      IdRoleGroup = rg.IdRoleGroup,
                                      IdSchool = rg.IdSchool
                                  }).FirstOrDefaultAsync(CancellationToken);

            if(CheckRole == null)
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

            var query = new List<GetSchoolEventInvolvementResult>();

            if(CheckRole.IdRoleGroup == "STD")
            {
                query = await (from eaa in _dbContext.Entity<TrEventActivityAward>()
                                   join ea in _dbContext.Entity<TrEventActivity>() on eaa.IdEventActivity equals ea.Id
                                   join e in _dbContext.Entity<TrEvent>() on ea.IdEvent equals e.Id
                                   join ed in _dbContext.Entity<TrEventDetail>() on ea.IdEvent equals ed.IdEvent
                                   join ac in _dbContext.Entity<MsActivity>() on ea.IdActivity equals ac.Id
                                   join aw in _dbContext.Entity<MsAward>() on eaa.IdAward equals aw.Id
                                   join hs in _dbContext.Entity<MsHomeroomStudent>() on eaa.IdHomeroomStudent equals hs.Id
                                   join s in _dbContext.Entity<MsStudent>() on hs.IdStudent equals s.Id
                                   where s.Id == param.IdUser
                                    
                                  select new GetSchoolEventInvolvementResult
                                  {
                                      Id = eaa.Id,
                                      EventId = e.Id,
                                      InvolvementId = eaa.Id,
                                      StudentId = s.Id,
                                      StudentName = s.FirstName + " " + s.LastName,
                                      EventName = e.Name,
                                      ActivityId = ac.Id,
                                      ActivityName = ac.Description,
                                      AwardId = aw.Id,
                                      AwardName = aw.Description,
                                      StartDate = ed.StartDate,
                                      EndDate = ed.EndDate,
                                      ApprovalSatatus = e.StatusEvent == "Approved" ? e.StatusEventAward : e.StatusEvent,
                                      CanApprove = e.StatusEvent.Contains("On Review") ? true : false,
                                      CanEdit = (e.StatusEvent == "Declined") ? true : false,
                                      CanDelete = (e.StatusEvent == "Declined") ? true : false,
                                      IsStudentInvolvement = e.IsStudentInvolvement
                                  })
                                  .Distinct()
                                  .ToListAsync(CancellationToken);

                query = query.Where(x => (x.IsStudentInvolvement == false) ? x.ApprovalSatatus == "Approved" : x.ApprovalSatatus != "").ToList();
            }
            else if(CheckRole.IdRoleGroup == "PRT")
            {
                query = await (from eaa in _dbContext.Entity<TrEventActivityAward>()
                                   join ea in _dbContext.Entity<TrEventActivity>() on eaa.IdEventActivity equals ea.Id
                                   join e in _dbContext.Entity<TrEvent>() on ea.IdEvent equals e.Id
                                   join ed in _dbContext.Entity<TrEventDetail>() on ea.IdEvent equals ed.IdEvent
                                   join ac in _dbContext.Entity<MsActivity>() on ea.IdActivity equals ac.Id
                                   join aw in _dbContext.Entity<MsAward>() on eaa.IdAward equals aw.Id
                                   join hs in _dbContext.Entity<MsHomeroomStudent>() on eaa.IdHomeroomStudent equals hs.Id
                                   join s in _dbContext.Entity<MsStudent>() on hs.IdStudent equals s.Id
                                //    join sp in _dbContext.Entity<MsStudentParent>() on s.Id equals sp.IdStudent into lsp
                                //    from xsp in lsp.DefaultIfEmpty()
                                   where s.Id == param.IdStudent && e.StatusEventAward == "Approved"
                                    
                                  select new GetSchoolEventInvolvementResult
                                  {
                                      Id = eaa.Id,
                                      EventId = e.Id,
                                      InvolvementId = eaa.Id,
                                      StudentId = s.Id,
                                      ParentId = param.IdUser,
                                      StudentName = s.FirstName + " " + s.LastName,
                                      EventName = e.Name,
                                      ActivityId = ac.Id,
                                      ActivityName = ac.Description,
                                      AwardId = aw.Id,
                                      AwardName = aw.Description,
                                      StartDate = ed.StartDate,
                                      EndDate = ed.EndDate,
                                      ApprovalSatatus = e.StatusEvent,
                                      CanApprove = e.StatusEvent.Contains("On Review") ? true : false,
                                      CanEdit = (e.StatusEvent == "Declined") ? true : false,
                                      CanDelete = (e.StatusEvent == "Declined") ? true : false,
                                      IsStudentInvolvement = e.IsStudentInvolvement
                                  })
                                  .Distinct()
                                  .ToListAsync(CancellationToken);
            }
            else{
                query = await (from eaa in _dbContext.Entity<TrEventActivityAward>()
                                    join ea in _dbContext.Entity<TrEventActivity>() on eaa.IdEventActivity equals ea.Id
                                    join e in _dbContext.Entity<TrEvent>() on ea.IdEvent equals e.Id
                                    join ed in _dbContext.Entity<TrEventDetail>() on ea.IdEvent equals ed.IdEvent
                                    join ac in _dbContext.Entity<MsActivity>() on ea.IdActivity equals ac.Id
                                    join aw in _dbContext.Entity<MsAward>() on eaa.IdAward equals aw.Id
                                    join hs in _dbContext.Entity<MsHomeroomStudent>() on eaa.IdHomeroomStudent equals hs.Id
                                    join s in _dbContext.Entity<MsStudent>() on hs.IdStudent equals s.Id
                                    join h in _dbContext.Entity<MsHomeroom>() on hs.IdHomeroom equals h.Id
                                    join ht in _dbContext.Entity<MsHomeroomTeacher>() on hs.IdHomeroom equals ht.IdHomeroom
                                    where ht.IdBinusian == param.IdUser && e.IsStudentInvolvement == true
                                    
                                    select new GetSchoolEventInvolvementResult
                                    {
                                        Id = eaa.Id,
                                        EventId = e.Id,
                                        InvolvementId = eaa.Id,
                                        StudentId = s.Id,
                                        StudentName = s.FirstName + " " + s.LastName,
                                        EventName = e.Name,
                                        ActivityId = ac.Id,
                                        ActivityName = ac.Description,
                                        AwardId = aw.Id,
                                        AwardName = aw.Description,
                                        StartDate = ed.StartDate,
                                        EndDate = ed.EndDate,
                                        ApprovalSatatus = e.StatusEvent,
                                        CanApprove = e.StatusEvent.Contains("On Review") ? true : false,
                                        CanEdit = (e.StatusEvent == "Declined") ? true : false,
                                        CanDelete = (e.StatusEvent == "Declined") ? true : false,
                                        IsStudentInvolvement = e.IsStudentInvolvement
                                    })
                                    .Distinct()
                                    .ToListAsync(CancellationToken);
            }

            if (!string.IsNullOrEmpty(param.IdActivity))
                    query = query.Where(x => x.ActivityId == param.IdActivity).ToList();

            if (!string.IsNullOrEmpty(param.IdAward))
                    query = query.Where(x => x.AwardId == param.IdAward).ToList();
            
            if(CheckRole.IdRoleGroup == "STD")
                    query = query.Where(x => x.StudentId == param.IdUser).ToList();

            if(CheckRole.IdRoleGroup == "PRT")
                    query = query.Where(x => x.ParentId == param.IdUser).ToList();

            if(!string.IsNullOrEmpty(param.Search))
                    query = query.Where(x => x.EventName.ToLower().Contains(param.Search.ToLower()) || x.ActivityName.ToLower().Contains(param.Search.ToLower()) || x.AwardName.ToLower().Contains(param.Search.ToLower())).ToList();

            if(!string.IsNullOrEmpty(param.StartDate))
                    query = query.Where(x => x.StartDate >= Convert.ToDateTime(param.StartDate)).ToList();

            if (!string.IsNullOrEmpty(param.EndDate))
                    query = query.Where(x => x.EndDate <= Convert.ToDateTime(param.EndDate)).ToList();

            //ordering
            switch (param.OrderBy)
            {
                case "EventName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.EventName).ToList()
                        : query.OrderBy(x => x.EventName).ToList();
                    break;
                case "StudentName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StudentName).ToList()
                        : query.OrderBy(x => x.StudentName).ToList();
                    break;
                case "Activity":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ActivityName).ToList()
                        : query.OrderBy(x => x.ActivityName).ToList();
                    break;
                case "Award":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AwardName).ToList()
                        : query.OrderBy(x => x.AwardName).ToList();
                    break;
                case "StartDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StartDate).ToList()
                        : query.OrderBy(x => x.StartDate).ToList();
                    break;
                case "EndDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.EndDate).ToList()
                        : query.OrderBy(x => x.EndDate).ToList();
                    break;
                case "ApprovalStatus":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ApprovalSatatus).ToList()
                        : query.OrderBy(x => x.ApprovalSatatus).ToList();
                    break;
            };

            var dataListPagination = query
                .SetPagination(param)
                .Select(x => new GetSchoolEventInvolvementResult
                {
                    Id = x.Id,
                    EventId = x.EventId,
                    InvolvementId = x.InvolvementId,
                    StudentId = x.StudentId,
                    StudentName = x.StudentName,
                    EventName = x.EventName,
                    ActivityId = x.ActivityId,
                    ActivityName = x.ActivityName,
                    AwardId = x.AwardId,
                    AwardName = x.AwardName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    ApprovalSatatus = x.ApprovalSatatus,
                    CanApprove = x.CanApprove,
                    CanEdit = x.CanEdit,
                    CanDelete = x.CanDelete,
                    IsStudentInvolvement = x.IsStudentInvolvement
                }).ToList();

            var count = param.CanCountWithoutFetchDb(dataListPagination.Count) 
                ? dataListPagination.Count 
                : query.Count;

            return Request.CreateApiResult2(dataListPagination as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
