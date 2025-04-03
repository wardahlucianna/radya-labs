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
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class GetSessionVenueForSubstitutionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetSessionVenueForSubstitutionHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSessionVenueForSubstitutionRequest>(nameof(GetSessionVenueForSubstitutionRequest.IdAcademicYear), nameof(GetSessionVenueForSubstitutionRequest.StartDate), nameof(GetSessionVenueForSubstitutionRequest.EndDate), nameof(GetSessionVenueForSubstitutionRequest.IsSession));

            var predicate = PredicateBuilder.Create<TrScheduleRealization2>(x => x.ScheduleDate >= param.StartDate && x.ScheduleDate <= param.EndDate && x.IdAcademicYear == param.IdAcademicYear);

            if(param.IdUserTeacher != null)
                predicate = predicate.And(x => param.IdUserTeacher.Contains(x.IdBinusian));

            if(param.IdUserSubstituteTeacher != null)
                predicate = predicate.And(x => param.IdUserSubstituteTeacher.Contains(x.IdBinusianSubtitute));
            
            if(!string.IsNullOrWhiteSpace(param.IdLevel))
                predicate = predicate.And(x => x.IdLevel == param.IdLevel);

            if(param.IdGrade != null)
                predicate = predicate.And(x => param.IdGrade.Contains(x.IdGrade));

            var query = _dbContext.Entity<TrScheduleRealization2>()
                            .Include(x => x.Venue)
                            .Where(predicate)
                            .Select(x => new { x.SessionID,  x.StartTime, x.EndTime, x.IdVenue, x.Venue.Description });

            if(param.IsSession)
            {
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
            else
            {
                IReadOnlyList<GetVenueByTeacherDateResult> items;
                items = await query
                    .Select(x => new GetVenueByTeacherDateResult
                        {
                            Id = x.IdVenue,
                            Code = x.IdVenue,
                            Description = x.Description
                        }
                    )
                    .Distinct()
                    .OrderBy(x => x.Code)
                    .ToListAsync(CancellationToken);

                items = items
                        .Select(x => new GetVenueByTeacherDateResult
                        {
                            Id = x.Id,
                            Code = x.Code,
                            Description = x.Description
                        }    
                    )
                    .OrderBy(x => x.Code)
                    .ToList();
            
                return Request.CreateApiResult2(items as object);
            }
            
        }
    }
}
