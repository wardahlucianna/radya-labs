using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AvailabilitySetting;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnSchedule.AvailabilitySetting.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.AvailabilitySetting
{
    public class AvailabilitySettingHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public AvailabilitySettingHandler(ISchedulingDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetAvailabilitySettingRequest>();
            var predicate = PredicateBuilder.Create<TrAvailabilitySetting>(x => x.IdUserTeacher == param.IdUserTeacher
                                                                                && x.Semester == param.Semester
                                                                                && x.IdAcademicYear == param.IdAcademicYear
                                                                            );

            var GetDays = await _dbContext.Entity<LtDay>()
                    .Where(e=>e.Id!="7" && e.Id != "6")
                    .ToListAsync(CancellationToken);

            var GetAvailabilitySetting = await _dbContext.Entity<TrAvailabilitySetting>()
              .Where(predicate).OrderBy(e=>e.StartTime).ToListAsync(CancellationToken);

            IReadOnlyList<IItemValueVm> items = default;
            items = GetDays
                .Select(e => new GetAvailabilitySettingResult
                    {
                        Days = e.Description,
                        IdUserTeacher = param.IdUserTeacher,
                        IdAcademicYear = param.IdAcademicYear,
                        Semester = param.Semester.ToString(),
                        Times = GetAvailabilitySetting
                            .Where(f => f.Day == e.Description)
                            .Select(f => (f.StartTime.ToString(@"hh\:mm")) + "-" + (f.EndTime.ToString(@"hh\:mm")))
                            .ToList()
                    })
                    .Distinct().ToList();

            return Request.CreateApiResult2(items);
        }

        protected async override Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddAvailabilitySettingRequest, AddAvailabilitySettingValidator>();

            var GetAvailabilitySetting = await _dbContext.Entity<TrAvailabilitySetting>()
               .Where(e => e.Day == body.Day
                       && e.IdAcademicYear == body.IdAcademicYear
                       && e.IdUserTeacher == body.IdUserTeacher
                       && e.Semester == body.Semester
               )
               .ToListAsync(CancellationToken);

            if (GetAvailabilitySetting.Any())
            {
                GetAvailabilitySetting.ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrAvailabilitySetting>().UpdateRange(GetAvailabilitySetting);
            }

            foreach (var item in body.AvailabilitySettings)
            {
                if (TimeSpan.Parse(item.EndTime) < TimeSpan.Parse(item.StartTime))
                    throw new BadImageFormatException("Cant backdate");

                var Exsis = body.AvailabilitySettings.Where(e =>
                                TimeSpan.Parse(e.StartTime) >= TimeSpan.Parse(item.StartTime) && TimeSpan.Parse(e.StartTime) < TimeSpan.Parse(item.EndTime)
                                || TimeSpan.Parse(e.EndTime) > TimeSpan.Parse(item.StartTime) && TimeSpan.Parse(e.EndTime) < TimeSpan.Parse(item.EndTime)
                                ).Count()>1
                                ? true
                                : false;

                if (Exsis)
                    throw new BadRequestException("Time cannot intersect");

                if ((TimeSpan.Parse(item.StartTime).Minutes != 30 && TimeSpan.Parse(item.StartTime).Minutes != 0) || (TimeSpan.Parse(item.EndTime).Minutes != 30 && TimeSpan.Parse(item.EndTime).Minutes != 0))
                    throw new BadRequestException("minute time must be 0 or 30");


                var AddAvailabilitySetting = new TrAvailabilitySetting
                {
                    Id = Guid.NewGuid().ToString(),
                    IdUserTeacher = body.IdUserTeacher,
                    IdAcademicYear = body.IdAcademicYear,
                    Semester = body.Semester,
                    Day = body.Day,
                    StartTime = TimeSpan.Parse(item.StartTime),
                    EndTime = TimeSpan.Parse(item.EndTime),
                };

                _dbContext.Entity<TrAvailabilitySetting>().Add(AddAvailabilitySetting);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
