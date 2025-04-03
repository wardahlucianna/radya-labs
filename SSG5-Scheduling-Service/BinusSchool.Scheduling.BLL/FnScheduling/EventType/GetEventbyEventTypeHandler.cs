using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.EventType;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;

namespace BinusSchool.Scheduling.FnSchedule.EventType
{
    public class GetEventbyEventTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetEventbyEventTypeHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
           
            var param = Request.ValidateParams<GetEventbyEventTypeRequest>(nameof(GetEventbyEventTypeRequest.IdAcademicYear), nameof(GetEventbyEventTypeRequest.IdEventType));

            var predicate = PredicateBuilder.Create<TrEvent>(x => x.IdAcademicYear == param.IdAcademicYear && x.IdEventType == param.IdEventType);

            var ReturnResult = _dbContext.Entity<TrEvent>()
                          .Where(predicate)
                          .OrderBy(a => a.Name)
                          .Select(a => new ItemValueVm()
                          {
                              Id = a.Id,
                              Description = a.Name
                          }).ToList();


            return Request.CreateApiResult2(ReturnResult as object);
        }
    }
}
