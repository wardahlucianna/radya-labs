using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnPeriod.Period
{
    public class CurrentAcademicYearHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public CurrentAcademicYearHandler(ISchoolDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<CurrentAcademicYearRequest>(nameof(CurrentAcademicYearRequest.IdSchool));

            DateTime today = _dateTime.ServerTime.Date;

            var data = new CurrentAcademicYearResult();

            var getActivePeriod = await _dbContext.Entity<MsPeriod>()
               .Include(x => x.Grade)
                   .ThenInclude(x => x.Level)
                       .ThenInclude(x => x.AcademicYear)
               .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool)
               .Where(x => today >= x.StartDate.Date)
               .Where(x => today <= x.EndDate.Date)
               .FirstOrDefaultAsync(CancellationToken);

            if (getActivePeriod != null)
            {
                data.Id = getActivePeriod?.Grade.Level.AcademicYear.Id;
                data.Code = getActivePeriod?.Grade.Level.AcademicYear.Code;
                data.Description = getActivePeriod?.Grade.Level.AcademicYear.Description;
                data.Semester = getActivePeriod?.Semester ?? 0;
            }
            else
            {
                var getLatestAcademicYear = await _dbContext.Entity<MsPeriod>()
                    .Include(x => x.Grade)
                        .ThenInclude(x => x.Level)
                            .ThenInclude(x => x.AcademicYear)
                    .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool)
                    .Where(x => x.EndDate.Date < today)
                    .OrderByDescending(a => a.Grade.Level.IdAcademicYear)
                        .ThenByDescending(a => a.Description)
                    .FirstOrDefaultAsync(CancellationToken);

                data.Id = getLatestAcademicYear?.Grade.Level.AcademicYear.Id;
                data.Code = getLatestAcademicYear?.Grade.Level.AcademicYear.Code;
                data.Description = getLatestAcademicYear?.Grade.Level.AcademicYear.Description;
                data.Semester = getLatestAcademicYear?.Semester ?? 0;
            }

            return Request.CreateApiResult2(data as object);
        }
    }
}
