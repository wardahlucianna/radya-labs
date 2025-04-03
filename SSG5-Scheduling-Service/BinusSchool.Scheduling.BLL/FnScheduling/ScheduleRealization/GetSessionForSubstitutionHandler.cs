using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization
{
    public class GetSessionForSubstitutionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetSessionForSubstitutionHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSessionForSubstitutionRequest>(nameof(GetSessionForSubstitutionRequest.IdAcademicYear), nameof(GetSessionForSubstitutionRequest.StartDate), nameof(GetSessionForSubstitutionRequest.EndDate));

            var predicate = PredicateBuilder.Create<TrScheduleRealization>(x => x.ScheduleDate >= param.StartDate && x.ScheduleDate <= param.EndDate && x.Homeroom.IdAcademicYear == param.IdAcademicYear);

            if(param.IdUserTeacher != null)
                predicate = predicate.And(x => param.IdUserTeacher.Contains(x.IdBinusian));

            if(param.IdUserSubstituteTeacher != null)
                predicate = predicate.And(x => param.IdUserSubstituteTeacher.Contains(x.IdBinusianSubtitute));
            
            if(!string.IsNullOrWhiteSpace(param.IdLevel))
                predicate = predicate.And(x => x.Homeroom.Grade.IdLevel == param.IdLevel);

            if(param.IdGrade != null)
                predicate = predicate.And(x => param.IdGrade.Contains(x.Homeroom.IdGrade));
            
            var query = _dbContext.Entity<TrScheduleRealization>()
                                 .Include(x => x.Homeroom).ThenInclude(x => x.Grade)
                                 .Where(predicate)
                                 .Select(x => new { x.SessionID,  x.StartTime, x.EndTime });

            IReadOnlyList<GetSessionForSubstitutionResult> items;
            items = await query
                .Select(x => new GetSessionForSubstitutionResult
                    {
                        SessionID = x.SessionID,
                        StartTime = x.StartTime,
                        EndTime = x.EndTime
                    }
                )
                .Distinct()
                .OrderBy(x => x.SessionID)
                .ToListAsync(CancellationToken);

            items = items
                    .Select(x => new GetSessionForSubstitutionResult
                    {
                        SessionID = x.SessionID,
                        StartTime = x.StartTime,
                        EndTime = x.EndTime
                    }    
                ).ToList();

            List<GetSessionForSubstitutionResult> itemSession = new List<GetSessionForSubstitutionResult>();

            var noSession = 1;
            foreach(var dataItems in items)
            {
                var dataSession = new GetSessionForSubstitutionResult
                {
                    Id = dataItems.SessionID.ToString(),
                    Code = dataItems.SessionID.ToString(),
                    Description = "Session "+noSession,
                    SessionID = dataItems.SessionID,
                    StartTime = dataItems.StartTime,
                    EndTime = dataItems.EndTime
                };

                itemSession.Add(dataSession);

                noSession++;
            }
            
           
            return Request.CreateApiResult2(itemSession as object);
        }
    }
}
