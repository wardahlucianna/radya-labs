using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetGradeHasGeneratedScheduleHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetGradeHasGeneratedScheduleHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGradeRequest>(nameof(GetGradeRequest.IdSchool));
            var predicate = PredicateBuilder.Create<TrGeneratedScheduleGrade>(x => param.IdSchool.Any(y => y == x.Grade.Level.AcademicYear.IdSchool));
            if (!string.IsNullOrEmpty(param.IdAcadyear))
                predicate = predicate.And(x => x.Grade.Level.IdAcademicYear == param.IdAcadyear);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Grade.IdLevel == param.IdLevel);
            var query =  _dbContext.Entity<TrGeneratedScheduleGrade>()
              .Include(x => x.Grade.Level.AcademicYear.School)
              .Where(predicate)
              .AsQueryable();

            var items = await query
                .GroupBy(x=>new
                {
                    x.Grade.Id,
                    x.Grade.Code,
                    x.Grade.Description,
                    idLevel = x.Grade.Level.Id,
                    codeLevel = x.Grade.Level.Code,
                    descLevel = x.Grade.Level.Description,
                    idAcadyear = x.Grade.Level.AcademicYear.Id,
                    codeAcadyear = x.Grade.Level.AcademicYear.Code,
                    descAcadyear = x.Grade.Level.AcademicYear.Description,
                    idSchool = x.Grade.Level.AcademicYear.School.Id,
                    codeSchool = x.Grade.Level.AcademicYear.School.Name,
                    descSchool = x.Grade.Level.AcademicYear.School.Description
                })
                .SetPagination(param)
                .Select(x => new GetGradeResult
                {
                    Id = x.Key.Id,
                    Code = x.Key.Code,
                    Description = x.Key.Description,
                    Acadyear = new CodeWithIdVm
                    {
                        Id = x.Key.idAcadyear,
                        Code = x.Key.codeAcadyear,
                        Description = x.Key.descAcadyear
                    },
                    Level = new CodeWithIdVm
                    {
                        Id = x.Key.idLevel,
                        Code = x.Key.codeLevel,
                        Description = x.Key.descLevel
                    },
                    School = new CodeWithIdVm
                    {
                        Id = x.Key.idSchool,
                        Code = x.Key.codeSchool,
                        Description = x.Key.descSchool
                    }
                }).ToListAsync();

            var count = param.CanCountWithoutFetchDb(items.Count)
               ? items.Count
               : await query.Select(x => x.Id).CountAsync();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(null));
        }
    }
}
