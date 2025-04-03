using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetWeekByGradeSubjectHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetWeekByGradeSubjectHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetWeekByGradeSubjectRequest>(
               nameof(GetWeekByGradeSubjectRequest.IdGrade));

            var query = _dbContext.Entity<MsGrade>()
                .Include(x => x.Lessons).ThenInclude(x => x.Subject)
                .Include(x => x.Lessons).ThenInclude(x => x.WeekVariant).ThenInclude(x => x.WeekVarianDetails).ThenInclude(x => x.Week)
                .Where(x => x.Id == param.IdGrade);

            var result = await query
                .Select(x => new GetWeekByGradeSubjectResult
                {
                    Subjects = x.Lessons.Select(y => new Subject
                    {
                        Id = y.Subject.Id,
                        Code = y.Subject.Code,
                        Description = y.Subject.Description,
                        WeekVariant = new WeekVariantSubject{
                            Id = y.WeekVariant.Id,
                            Code = y.WeekVariant.Code,
                            Description = y.WeekVariant.Description,
                            Weeks = y.WeekVariant.WeekVarianDetails.Select(z => new Week{
                                Id = z.Week.Id,
                                Code = z.Week.Code,
                                Description = z.Week.Description
                            }).ToList()
                        }
                    }).ToList(),
                    IdGrade = x.Id
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(result as object);
        }
    }
}
