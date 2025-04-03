using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Scheduling.FnSchedule.Award.Validator;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Configuration;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Configurations;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
//using ImpromptuInterface;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class SetSchoolEventApprovalStatusHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IStringLocalizer _localizer;
        private readonly IConfiguration _configuration;
        private readonly IEventSchool _eventSchoolService;

        public SetSchoolEventApprovalStatusHandler(ISchedulingDbContext schedulingDbContext,
         IStringLocalizer localizer,
         IConfiguration configuration,
         IEventSchool eventSchoolService)
        {
            _dbContext = schedulingDbContext;
            _localizer = localizer;
            _configuration = configuration;
            _eventSchoolService = eventSchoolService;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SetSchoolEventApprovalStatusRequest, SetSchoolEventApprovalStatusValidator>();

            var apiConfig = _configuration.GetSection("BinusSchoolService").Get<BinusSchoolApiConfiguration2>();

            var CheckRole = await (from a in _dbContext.Entity<MsUser>()
                                   join r in _dbContext.Entity<MsUserRole>() on a.Id equals r.IdUser
                                   join rg in _dbContext.Entity<LtRole>() on r.IdRole equals rg.Id
                                   where a.Id == body.IdUser

                                   select new LtRole
                                   {
                                       IdRoleGroup = rg.IdRoleGroup
                                   }).FirstOrDefaultAsync(CancellationToken);

            if (CheckRole == null)
                throw new BadRequestException($"User role not found");

            if (CheckRole.IdRoleGroup == "PRT")
            {
                var dataUserEvent = await _dbContext.Entity<TrUserEvent>()
                    .Where(x => x.Id == body.Id)
                    .FirstOrDefaultAsync(CancellationToken);

                if (dataUserEvent is null)
                    throw new BadRequestException($"Data user event not found");

                dataUserEvent.IsApproved = body.IsApproved;
                dataUserEvent.Reason = body.Reason;
            }
            else
            {
                var dataEvent = await _dbContext.Entity<TrEvent>()
                        .Include(x => x.EventDetails)
                        .Include(x => x.EventIntendedFor).ThenInclude(e => e.EventIntendedForLevelStudents)
                        .Include(x => x.EventIntendedFor).ThenInclude(e => e.EventIntendedForGradeStudents)
                        .Include(x => x.EventIntendedFor).ThenInclude(e => e.EventIntendedForPersonalParents)
                        .Include(x => x.EventIntendedFor).ThenInclude(e => e.EventIntendedForAttendanceStudents)
                        .Include(x => x.EventType)
                        .Include(x => x.AcademicYear).ThenInclude(x => x.School)
                        .Include(x => x.EventCoordinators).ThenInclude(x => x.User)
                        .Include(x => x.EventApprovers)
                        .Include(x => x.EventAwardApprovers)
                        .Include(x => x.EventApprovals)
                    .Where(x => x.Id == body.IdEvent)
                    .FirstOrDefaultAsync(CancellationToken);

                if (body.IsSectionEventSetting == true)
                {
                    if (dataEvent.StatusEvent == "Approved")
                    {
                        int stateEventAward = 1;

                        if (dataEvent.StatusEventAward == "On Review (1)")
                        {
                            stateEventAward = 1;
                        }
                        else
                        {
                            stateEventAward = 2;
                        }

                        if (dataEvent is null)
                            throw new BadRequestException($"Data event not found");

                        if (dataEvent.EventAwardApprovers is null)
                            throw new BadRequestException($"Event award approver not found");

                        var approverUser = dataEvent.EventAwardApprovers.Where(x => x.IdUser == body.IdUser).FirstOrDefault();

                        if (approverUser == null)
                            throw new NotFoundException("User not allowed to approve/decline");

                        var dataEventApproval = dataEvent.EventApprovals.Where(x => x.IdUser == body.IdUser).FirstOrDefault();

                        if (dataEventApproval.State != stateEventAward)
                            throw new NotFoundException("User not allowed to approve/decline this state");

                        if (stateEventAward == 1 && dataEvent.EventAwardApprovers.Count() > 1)
                        {
                            _dbContext.Entity<HTrEventApproval>().Add(new HTrEventApproval
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdEvent = body.IdEvent,
                                Section = "Award",
                                State = 1,
                                IsApproved = body.IsApproved,
                                Reason = body.Reason != null ? body.Reason : null,
                                IdUser = body.IdUser
                            });

                            dataEvent.IsShowOnCalendarAcademic = body.IsShowAcademicCalender;
                            dataEvent.StatusEventAward = body.IsApproved == true ? "On Review (2)" : "Declined";
                            dataEvent.DescriptionEventAward = body.IsApproved == true ? "Record of Involvement is On Review" : "Record of Involvement Declined";
                        }
                        else
                        {
                            _dbContext.Entity<HTrEventApproval>().Add(new HTrEventApproval
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdEvent = body.IdEvent,
                                Section = "Award",
                                State = stateEventAward == 1 ? 1 : 2,
                                IsApproved = body.IsApproved,
                                Reason = body.Reason != null ? body.Reason : null,
                                IdUser = body.IdUser
                            });

                            dataEvent.IsShowOnCalendarAcademic = body.IsShowAcademicCalender;
                            dataEvent.StatusEventAward = body.IsApproved == true ? "Approved" : "Declined";
                            dataEvent.DescriptionEventAward = body.IsApproved == true ? "Event Settings and Record of Involvement Approved" : "Record of Involvement Declined";
                        }
                    }
                    else
                    {
                        int stateEvent = 1;

                        if (dataEvent.StatusEvent == "On Review (1)")
                        {
                            stateEvent = 1;
                        }
                        else
                        {
                            stateEvent = 2;
                        }

                        if (dataEvent is null)
                            throw new BadRequestException($"Data event not found");

                        if (dataEvent.EventApprovers is null)
                            throw new BadRequestException($"Event approver not found");

                        var approverUser = dataEvent.EventApprovers.Where(x => x.IdUser == body.IdUser).FirstOrDefault();

                        if (approverUser == null)
                            throw new NotFoundException("User not allowed to approve/decline");

                        var dataEventApproval = dataEvent.EventApprovals.Where(x => x.IdUser == body.IdUser).FirstOrDefault();

                        if (dataEventApproval.State != stateEvent)
                            throw new NotFoundException("User not allowed to approve/decline this state");
                        if (stateEvent == 1 && dataEvent.EventApprovers.Count() > 1)
                        {
                            _dbContext.Entity<HTrEventApproval>().Add(new HTrEventApproval
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdEvent = body.IdEvent,
                                Section = "Event",
                                State = 1,
                                IsApproved = body.IsApproved,
                                Reason = body.Reason != null ? body.Reason : null,
                                IdUser = body.IdUser
                            });

                            dataEvent.IsShowOnCalendarAcademic = body.IsShowAcademicCalender;
                            dataEvent.StatusEvent = body.IsApproved == true ? "On Review (2)" : "Declined";
                            dataEvent.DescriptionEvent = body.IsApproved == true ? "Event Settings is On Review" : "Event Declined";
                        }
                        else
                        {
                            _dbContext.Entity<HTrEventApproval>().Add(new HTrEventApproval
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdEvent = body.IdEvent,
                                Section = "Event",
                                State = stateEvent == 1 ? 1 : 2,
                                IsApproved = body.IsApproved,
                                Reason = body.Reason != null ? body.Reason : null,
                                IdUser = body.IdUser
                            });

                            dataEvent.IsShowOnCalendarAcademic = body.IsShowAcademicCalender;
                            dataEvent.StatusEvent = body.IsApproved == true ? "Approved" : "Declined";
                            dataEvent.DescriptionEvent = body.IsApproved == true ? "Event Settings Approved" : "Event Declined";

                            var checkDataAward = await _dbContext.Entity<TrEventActivityAward>()
                                .Include(x => x.EventActivity)
                                    .ThenInclude(x => x.Event)
                                .Where(x => x.EventActivity.Event.Id == body.IdEvent)
                                .FirstOrDefaultAsync(CancellationToken);
                            if (checkDataAward != null)
                            {
                                dataEvent.StatusEventAward = body.IsApproved == true ? "On Review (1)" : null;
                                dataEvent.DescriptionEventAward = body.IsApproved == true ? "Record of Involvement is On Review" : null;
                            }

                            //coordinator event

                            // var dataDetailEven = "<tr>" +
                            //             "<td>"+ dataEvent.Name + "</td>" +
                            //             "<td>" + dataEvent.EventType.Description + "</td>" +
                            //             "<td>" + dataEvent.Place + "</td>" +
                            //             "<td>startda"+"</td>" +
                            //             "<td>enddate"+"</td>" +
                            //             "<td>" + dataEvent.Objective + "</td>" +
                            //             "<td>linknya" +"</td>" +
                            //             "<td>createdeby"+ "</td>" +
                            //             "<td> Link </td>" +
                            //         "</tr>";

                            var dictionary = new Dictionary<string, object>
                            {
                                { "CoordinatorName", dataEvent.EventCoordinators.First().User.DisplayName },
                                { "EventName", dataEvent.Name },
                                { "EventTypeName", dataEvent.EventType.Description },
                                { "Place", dataEvent.Place },
                                { "StartDate", "startdatenya" },
                                { "EndDate", "enddatenya" },
                                { "EventObjectives", dataEvent.Objective },
                                { "LinkUrl", "https://bss-webclient.azurewebsites.net/" },
                                { "IdEvent", dataEvent.Id },
                                { "TeacherName", dataEvent.EventCoordinators.First().User.DisplayName },
                                { "SchoolName", dataEvent.AcademicYear.School.Description },
                                // { "Data", dataDetailEven },
                            };

                            // send notification
                            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                            {
                                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "EM1")
                                {
                                    IdRecipients = new[] { dataEvent.EventCoordinators.First().IdUser },
                                    KeyValues = dictionary
                                });
                                collector.Add(message);
                            }
                        }
                    }

                    try
                    {
                        // send history event
                        _ = await _eventSchoolService
                            .AddHistoryEvent(new AddHistoryEventRequest
                            {
                                IdEvent = dataEvent.Id,
                                IdUser = body.IdUser,
                                ActionType = body.IsApproved == true ? "Event Approved" : "Event Settings Declined"
                            });

                        _dbContext.Entity<TrEvent>().Update(dataEvent);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
                else
                {
                    var dataEventAwardApprover = await _dbContext.Entity<TrEventAwardApprover>()
                    .Where(x => x.Id == body.IdEvent)
                    .ToListAsync(CancellationToken);
                    if (dataEventAwardApprover is null)
                        throw new BadRequestException($"Event award approver not found");

                    if (dataEventAwardApprover.Any(r => r.IdUser != body.IdUser))
                        throw new NotFoundException("The user not allowed to approve/decline");

                    _dbContext.Entity<HTrEventApproval>().Add(new HTrEventApproval
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEvent = body.IdEvent,
                        Section = "Award",
                        State = 1,
                        IsApproved = body.IsApproved,
                        Reason = body.Reason != null ? body.Reason : null,
                        IdUser = body.IdUser
                    });

                    dataEvent.IsShowOnCalendarAcademic = body.IsShowAcademicCalender;
                    dataEvent.StatusEvent = body.IsApproved == true ? "Approved" : "Declined";
                    dataEvent.DescriptionEvent = body.IsApproved == true ? "Event Settings and Record of Involvement Approved" : "Record of Involvement Declined";
                    dataEvent.StatusEventAward = body.IsApproved == true ? "Approved" : "Declined";
                    dataEvent.DescriptionEventAward = body.IsApproved == true ? "Event Settings and Record of Involvement Approved" : "Record of Involvement Declined";

                    _dbContext.Entity<TrEvent>().Update(dataEvent);

                    try
                    {
                        // send history event
                        _ = _eventSchoolService
                            .AddHistoryEvent(new AddHistoryEventRequest
                            {
                                IdEvent = dataEvent.Id,
                                IdUser = body.IdUser,
                                ActionType = body.IsApproved == true ? "Event Settings and Record of Involvement Approved" : "Record of Involvement Declined"
                            });
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }


                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            var lastStatusEvent = await _dbContext.Entity<TrEvent>()
                    .Where(x => x.Id == body.IdEvent)
                    .FirstOrDefaultAsync(CancellationToken);

            if (lastStatusEvent.StatusEvent == "Approved" && lastStatusEvent.StatusEventAward == "Approved")
            {
                var JobsEventUpdateGenerate = new CodeWithIdVm
                {
                    Id = body.Id,
                    Code = "Approved"
                };

                if (KeyValues.ContainsKey("JobsEventUpdateGenerate"))
                {
                    KeyValues.Remove("JobsEventUpdateGenerate");
                }
                KeyValues.Add("JobsEventUpdateGenerate", JobsEventUpdateGenerate);
                var Notification = QueueEventUpdateGenerate(KeyValues, AuthInfo);
            }

            return Request.CreateApiResult2();
        }

        public static List<GetHomeroom> GetMovingStudent(List<GetHomeroom> listStudentEnrollmentUnion, DateTime scheduleDate, string semester, string idLesson, bool isFormCalendar)
        {

            var listStudentEnrollmentByDate = listStudentEnrollmentUnion
                                                .Where(e => e.EffectiveDate.Date <= scheduleDate.Date && e.Semester.ToString() == semester)
                                                .ToList();

            var listIdHomeroomStudentEnrollment = listStudentEnrollmentByDate.Select(e => e.IdHomeroomStudentEnrollment).Distinct().ToList();

            var listStudentEnrollmentNew = new List<GetHomeroom>();
            foreach (var idHomeroomStudentEnrollment in listIdHomeroomStudentEnrollment)
            {
                if (isFormCalendar)
                {
                    var listStudentEnrollment = listStudentEnrollmentByDate
                                         .Where(e => e.IdHomeroomStudentEnrollment == idHomeroomStudentEnrollment)
                                         .ToList();

                    var studentEnrollmentFirst = listStudentEnrollment.FirstOrDefault();
                    var studentEnrollmentLast = listStudentEnrollment.LastOrDefault();

                    if (studentEnrollmentLast.IdLesson == idLesson && !studentEnrollmentLast.IsDelete)
                    {
                        listStudentEnrollmentNew.Add(studentEnrollmentLast);
                    }
                }
                else
                {
                    var studentEnrollment = listStudentEnrollmentByDate
                                           .Where(e => e.IdHomeroomStudentEnrollment == idHomeroomStudentEnrollment)
                                           .LastOrDefault();

                    if (studentEnrollment.IdLesson == idLesson && !studentEnrollment.IsDelete)
                    {
                        listStudentEnrollmentNew.Add(studentEnrollment);
                    }
                }
                
            }

            return listStudentEnrollmentNew;
        }

        public static string QueueEventUpdateGenerate(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "JobsEventUpdateGenerate").Value;
            var JobsEventUpdateGenerate = JsonConvert.DeserializeObject<CodeWithIdVm>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "UGBE")
                {
                    KeyValues = KeyValues,
                });
                collector.Add(message);
            }

            return "";
        }
    }
}
