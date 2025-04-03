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
    public class GetVenueForSubstitutionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetVenueForSubstitutionHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetVenueForSubstitutionRequest>(nameof(GetVenueForSubstitutionRequest.IdAcademicYear), nameof(GetVenueForSubstitutionRequest.StartDate), nameof(GetVenueForSubstitutionRequest.EndDate));

            var predicate = PredicateBuilder.Create<TrScheduleRealization>(x => x.ScheduleDate >= param.StartDate && x.ScheduleDate <= param.EndDate && x.Homeroom.IdAcademicYear == param.IdAcademicYear);

            var columns = new[] { "building", "code"};
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "venue.building.code" },
                { columns[1], "venue.code" }
            };

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
                                 .Include(x => x.Venue)
                                 .Where(predicate)
                                 .Select(x => new { x.Venue.Id,  x.Venue.Code, x.Venue.Description });

            IReadOnlyList<GetVenueByTeacherDateResult> items;
            items = await query
                .Select(x => new GetVenueByTeacherDateResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description
                    }
                )
                .Distinct()
                .OrderBy(x => x.Description.Length).ThenBy(x => x.Description)
                .ToListAsync(CancellationToken);

            items = items
                    .Select(x => new GetVenueByTeacherDateResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description
                    }    
                )
                .OrderBy(x => x.Description.Length).ThenBy(x => x.Description)
                .ToList();
           
            return Request.CreateApiResult2(items as object);
        }
    }
}
