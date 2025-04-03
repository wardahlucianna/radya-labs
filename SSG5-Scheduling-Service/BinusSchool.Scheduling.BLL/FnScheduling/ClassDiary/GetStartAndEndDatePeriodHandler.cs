using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.ClassDiary.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiary
{
    public class GetStartAndEndDatePeriodHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetStartAndEndDatePeriodHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStartAndEndDatePeriodRequest>();


            var GetPeriode = await (_dbContext.Entity<MsPeriod>()
                                        .Include(e=>e.Grade).ThenInclude(e=>e.Level).ThenInclude(e=>e.AcademicYear)
                                        .Where(e=>e.Grade.Level.IdAcademicYear==param.AcademicYearId && e.Grade.Id==param.GradeId && e.Semester==param.Semester)
                                        .Select(e => new GetStartAndEndDatePeriodResult
                                        {
                                            StartDate = e.StartDate,
                                            EndDate = e.EndDate,
                                        })
                                      ).ToListAsync(CancellationToken);

            var ReturnResult = new GetStartAndEndDatePeriodResult
            {
                StartDate = GetPeriode.Min(e => e.StartDate),
                EndDate = GetPeriode.Max(e => e.EndDate),
            };

            return Request.CreateApiResult2(ReturnResult as object);
        }

    }
}
