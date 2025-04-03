using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Session;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.Session.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Session
{
    public class SessionHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public SessionHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsSession>()
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas)
            {
                data.IsActive = false;
                _dbContext.Entity<MsSession>().Update(data);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<MsSession>()
                .Include(p => p.GradePathway).ThenInclude(p => p.GradePathwayDetails).ThenInclude(p => p.Pathway)
                .Include(p => p.GradePathway).ThenInclude(p => p.Grade)
                .Include(p => p.Day)
                .Select(x => new GetPathwayResult
                {
                    Id = x.Id,
                    Alias = x.Alias,
                    Name = x.Name,
                    SessionId = x.SessionID,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    DurationInMinutes = x.DurationInMinutes,
                    SchoolDay = x.Day.Description,
                    Pathway = string.Join("-", x.GradePathway.GradePathwayDetails.Select(p => p.Pathway.Description)),
                    Grade = x.GradePathway.Grade.Description,
                    Acadyear = x.GradePathway.Grade.Level.AcademicYear.Description
                })
                .FirstOrDefaultAsync(x => x.Id == id, CancellationToken);

            return Request.CreateApiResult2(data as object);
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            // use GetSessionHandler.cs instead
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddSessionRequest, AddSessionValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var existSessions = await _dbContext.Entity<MsSession>()
                .Where(x => x.IdSessionSet == body.IdSessionSet && body.IdDay.Contains(x.IdDay) && body.IdGradePathway.Contains(x.IdGradePathway))
                .ToListAsync(CancellationToken);

            foreach (var itemSession in body.IdGradePathway)
            {
                foreach (var itemDay in body.IdDay)
                {
                    // init default session id to 1
                    var incrementedSessionId = 1;
                    var existSession = existSessions.OrderByDescending(x => x.SessionID).Where(x => x.IdDay == itemDay && x.IdGradePathway == itemSession).Select(x => x.SessionID).FirstOrDefault();

                    // if previous any, increment default session id with previous session id
                    if (existSession != 0)
                        incrementedSessionId += existSession;

                    var param = new MsSession
                    {
                        Id = !string.IsNullOrEmpty(body.Id) ? body.Id : Guid.NewGuid().ToString(),
                        IdSessionSet = body.IdSessionSet,
                        IdDay = itemDay,
                        IdGradePathway = itemSession,
                        SessionID = incrementedSessionId,
                        Name = body.Name,
                        Alias = body.Alias,
                        DurationInMinutes = body.DurationInMinutes,
                        StartTime = TimeSpan.Parse(body.StartTime),
                        EndTime = TimeSpan.Parse(body.EndTime),
                        UserIn = AuthInfo.UserId
                    };

                    _dbContext.Entity<MsSession>().Add(param);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync();

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateSessionRequest, UpdateSessionValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<MsSession>().FirstOrDefaultAsync(p => p.Id == body.Id);
            if (data == null)
            {
                throw new NotFoundException("Session Not Found");
            }

            data.Name = body.Name;
            data.Alias = body.Alias;
            data.DurationInMinutes = body.DurationInMinutes;
            data.StartTime = TimeSpan.Parse(body.StartTime);
            data.EndTime = TimeSpan.Parse(body.EndTime);
            data.UserUp = AuthInfo.UserId;

            _dbContext.Entity<MsSession>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync();

            return Request.CreateApiResult2();
        }
    }
}
