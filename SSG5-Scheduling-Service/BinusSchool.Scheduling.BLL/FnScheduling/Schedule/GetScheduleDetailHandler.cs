using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Employee.FnStaff;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Schedule
{
    public class GetScheduleDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetScheduleDetailHandler(
            ISchedulingDbContext dbContext,
            IApiService<IMetadata> metadataService,
            IApiService<IVenue> venueService,
            IApiService<ISession> sessionService,
            IApiService<IDay> dayservices,
            IApiService<IStaff> staffServices)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            if (!KeyValues.TryGetValue("id", out var id))
                throw new BadRequestException("Not Found");

            var result = await _dbContext.Entity<MsSchedule>()
                        .Include(x => x.Lesson)
                        .Include(x => x.WeekVarianDetail).ThenInclude(x => x.Week)
                        .Include(x => x.AscTimetableSchedules).ThenInclude(x => x.AscTimetable)
                        .Include(x => x.Day)
                        .Include(x => x.Lesson).ThenInclude(x => x.Subject)
                        .Include(x => x.Venue).ThenInclude(x => x.Building).ThenInclude(x => x.School)
                        .Include(x => x.User)
                        .Where(x => x.Id == id.ToString())
                        .Select(x => new
                        {
                            IdSchedule = x.Id,
                            IdLesson = x.IdLesson,
                            Day = new CodeWithIdVm
                            {
                                Id = x.Day.Id,
                                Code = x.Day.Code,
                                Description = x.Day.Description
                            },
                            Session = new CodeWithIdVm
                            {
                                Id = x.IdSession,
                                Code = x.Sessions.SessionID.ToString(),
                                Description = x.Sessions.SessionID.ToString()
                            },
                            ClassID = x.Lesson.ClassIdGenerated,
                            Subject = x.Lesson.Subject.Description,
                            Teacher = new CodeWithIdVm
                            {
                                Id = x.User.IdBinusian,
                                Code = x.User.ShortName,
                                Description = !string.IsNullOrEmpty(x.User.FirstName) ? x.User.FirstName: x.User.LastName
                            },
                            Venue = new CodeWithIdVm
                            {
                                Id = x.Venue.Id,
                                Code = x.Venue.Id,
                                Description = string.Format("{0} - {1}", x.Venue.Building.Code, x.Venue.Code)
                            },
                            Week = new DataModelGeneral
                            {
                                Id = x.WeekVarianDetail.Id,
                                Code = x.WeekVarianDetail.Week.Code,
                                Description = x.WeekVarianDetail.Week.Description,
                                IdFromMasterData = x.WeekVarianDetail.Week.Id,
                                DataIsUseInMaster = true
                            }
                        })
                        .SingleOrDefaultAsync();
            if (result == null)
                throw new BadRequestException("Schedule Not Found");

            return Request.CreateApiResult2(result as object);
        }
    }
}
