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
    public class GetClassIdBYTeacherDateHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetClassIdBYTeacherDateHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetClassIdByTeacherDateRequest>(nameof(GetClassIdByTeacherDateRequest.IdAcademicYear), nameof(GetClassIdByTeacherDateRequest.StartDate), nameof(GetClassIdByTeacherDateRequest.EndDate), nameof(GetClassIdByTeacherDateRequest.idUser));

            var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => x.IsGenerated && (x.IdUser == param.idUser || x.IdBinusianOld == param.idUser) && x.ScheduleDate >= param.StartDate && x.ScheduleDate <= param.EndDate && x.Homeroom.IdAcademicYear == param.IdAcademicYear);

            var query = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                 .Where(predicate)
                                 .Select(x => new { x.ClassID}).Distinct().ToListAsync(CancellationToken);

            IReadOnlyList<GetClassIdByTeacherDateResult> items;

            items = query
                    .Select(x => new GetClassIdByTeacherDateResult
                    {
                        Id = x.ClassID,
                        Code = x.ClassID,
                        Description = x.ClassID
                    }    
                ).ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
