using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AvailabilitySetting;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.AvailabilitySetting
{
    public class DetailAvailabilitySettingHandler : FunctionsHttpSingleHandler
    {

        private readonly ISchedulingDbContext _dbContext;

        public DetailAvailabilitySettingHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<DetailAvailabilitySettingRequest>();

            var GetAvailabilitySetting = await _dbContext.Entity<TrAvailabilitySetting>()
                .Where(e => e.Day == param.Day
                        && e.IdAcademicYear == param.IdAcademicYear
                        && e.IdUserTeacher == param.IdUserTeacher
                        && e.Semester == param.Semester
                )
                .ToListAsync(CancellationToken);

            var items = new DetailAvailabilitySettingResult
            {
                Day = param.Day,
                IdAcademicYear = param.IdAcademicYear,
                IdUserTeacher = param.IdUserTeacher,
                Semseter = param.Semester,
                Times = GetAvailabilitySetting.Select(e => new Time
                {
                    StartTime = e.StartTime.ToString(@"hh\:mm"),
                    EndTime = e.EndTime.ToString(@"hh\:mm")
                }).ToList(),
            };



            return Request.CreateApiResult2(items as object);
        }
    }
}
