using System.Collections.Generic;
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
using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Session;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Schedule
{
    public class GetScheduleHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        
        public GetScheduleHandler(ISchedulingDbContext dbContext,
            IApiService<IMetadata> metadataService,
            IApiService<ISession> sessionService,
            IApiService<IDay> dayservices,
            IApiService<IStaff> staffServices)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetScheduleRequest>(nameof(GetScheduleRequest.IdAscTimetable));

            var dataChek = await _dbContext.Entity<TrAscTimetable>()
                        .Include(x => x.AscTimetableLessons).ThenInclude(x => x.Lesson).ThenInclude(x => x.LessonTeachers)
                        .Where(x => x.Id == param.IdAscTimetable)
                        .SingleOrDefaultAsync();

            if (dataChek == null)
                throw new BadRequestException("Asc Time table Not Found");

            if (!dataChek.AscTimetableLessons.Any())
                throw new BadRequestException("Error loading data because there is no data lesson");

            dataChek.AscTimetableSchedules = await _dbContext.Entity<TrAscTimetableSchedule>()
                                .Include(x => x.Schedule).ThenInclude(x => x.WeekVarianDetail).ThenInclude(x => x.Week)
                                .Include(x => x.Schedule).ThenInclude(x => x.Lesson).ThenInclude(x => x.LessonPathways).ThenInclude(x => x.HomeroomPathway)
                                .Include(x => x.Schedule).ThenInclude(x => x.Venue)
                                .Include(x => x.Schedule).ThenInclude(x => x.User)
                                .Where(x => x.IdAscTimetable == param.IdAscTimetable)
                                .Where(x => x.Schedule.IsActive)
                                .ToListAsync();

            if (!dataChek.AscTimetableSchedules.Any())
                throw new BadRequestException("Error loading data because there is no data schedule");

            var IdSessionSet = dataChek.IdSessionSet;
            var getSessions = await _dbContext.Entity<MsSession>()
                        .Include(p => p.GradePathway)
                        .ThenInclude(p => p.GradePathwayDetails)
                        .ThenInclude(p => p.Pathway)
                        .Include(p => p.GradePathway)
                        .ThenInclude(p => p.Grade)
                        .Include(p => p.Day)
                        .Where(p => p.IdSessionSet == IdSessionSet)
                        .Select(p => new GetSessionAscTimetableResult
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Alias = p.Alias,
                            DaysCode = p.Day.Code,
                            DaysName = p.Day.Description,
                            SessionId = p.SessionID,
                            StartTime = p.StartTime,
                            EndTime = p.EndTime,
                            DurationInMinutes = p.DurationInMinutes,
                            Pathway = string.Join("-", p.GradePathway.GradePathwayDetails.Select(x => x.Pathway.Description)),
                            Grade = p.GradePathway.Grade.Description,
                        }).ToListAsync();

            var getDays = _dbContext.Entity<LtDay>()
                 .Select(x => new CodeWithIdVm
                 {
                     Id = x.Id,
                     Code = x.Code,
                     Description = x.Description
                 }).ToList();

            return Request.CreateApiResult2(GetSchedules(dataChek.AscTimetableSchedules,getSessions.ToList(),getDays.ToList(),param) as object);
        }

        private List<ScheduleVm> GetSchedules(ICollection<TrAscTimetableSchedule> schedules,
                                              List<GetSessionAscTimetableResult> sessions,
                                              List<CodeWithIdVm> days,
                                              GetScheduleRequest param)
        {
            var result = new List<ScheduleVm>();
            var query = from sch in schedules
                        join period in sessions on sch.Schedule.IdSession equals period.Id
                        join daysday in days on sch.Schedule.IdDay equals daysday.Id
                        where sch.Schedule.IsActive == true
                        && sch.Schedule.Lesson.IdGrade == param.IdGrade
                        && sch.Schedule.Lesson.LessonPathways.Any(y => y.HomeroomPathway.IdHomeroom == param.IdHomeroom)
                        && sch.Schedule.Semester == param.Semester
                        && sch.Schedule.IsActive == true
                        orderby daysday.Code ascending
                        select new { Schedule = sch, session = period, days = daysday };

            var scheduleGroup = query.GroupBy(p => new { IdSessionSet = p.Schedule.AscTimetable.IdSessionSet, Session = p.session.SessionId }).ToList();
            foreach (var item in scheduleGroup.OrderBy(x => x.Key.Session))
            {
                var data = new ScheduleVm
                {

                    IdSessionSet = item.Key.IdSessionSet,
                    Session = item.Key.Session.ToString()
                };
                foreach (var itemSchedule in days)
                {
                    var scheduleValue = new ScheduleValueVM
                    {
                        ListSchedule = new List<ScheduleDataVM>(),
                        Days = new ScheduleDayVM()
                        {
                            IdSession = itemSchedule != null ? item.Where(x => x.Schedule.Schedule.IdDay == itemSchedule.Id).Select(x => x.Schedule.Schedule.IdSession).FirstOrDefault() : "",
                            Id = itemSchedule != null ? itemSchedule.Id : "",
                            Code = itemSchedule != null ? itemSchedule.Code : "",
                            Description = itemSchedule != null ? itemSchedule.Description : "",
                            IdFromMasterData = itemSchedule != null ? itemSchedule.Id : "",
                            DataIsUseInMaster = itemSchedule != null,
                        }
                    };

                    var datas = item.GroupBy(p => p.days).Where(p => p.Key.Id == itemSchedule.Id).SelectMany(p => p.Select(p => p)).ToList();

                    foreach (var listSchedule in datas.OrderBy(x => x.session.StartTime))
                    {
                        var modelList = new ScheduleDataVM
                        {
                            IdDataFromMaster = listSchedule.Schedule.IdSchedule,
                            DataIsUseInMaster = true,
                            LesonId = listSchedule.Schedule.Schedule.IdLesson,
                            StartTime = listSchedule.session.StartTime.ToString(@"h\:mm"),
                            EndTime = listSchedule.session.EndTime.ToString(@"h\:mm"),
                            ClassID = listSchedule.Schedule.Schedule.Lesson.ClassIdGenerated,
                            Teacher = new DataModelGeneral
                            {
                                Id = listSchedule.Schedule.Schedule.User.IdBinusian,
                                Code = listSchedule.Schedule.Schedule.User.ShortName,
                                Description = !string.IsNullOrEmpty(listSchedule.Schedule.Schedule.User.FirstName) ? listSchedule.Schedule.Schedule.User.FirstName : listSchedule.Schedule.Schedule.User.LastName,
                                IdFromMasterData = listSchedule.Schedule.Schedule.User.IdBinusian,
                                DataIsUseInMaster = true,
                            },
                            Weeks = listSchedule.Schedule.Schedule.WeekVarianDetail.Week.Description,
                            Venue = new DataModelGeneral
                            {
                                Id = listSchedule.Schedule.Schedule.IdVenue,
                                Code = listSchedule.Schedule.Schedule.Venue.Code,
                                Description = listSchedule.Schedule.Schedule.Venue.Description,
                                IdFromMasterData = listSchedule.Schedule.Schedule.IdVenue,
                                DataIsUseInMaster = true,
                            }
                        };

                        scheduleValue.ListSchedule.Add(modelList);
                    }
                    data.Schedule.Add(scheduleValue);
                }
                result.Add(data);
            }
            return result;
        }
    }
}
