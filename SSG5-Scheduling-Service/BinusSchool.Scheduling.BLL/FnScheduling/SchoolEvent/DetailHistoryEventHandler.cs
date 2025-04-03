using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Constants;
using BinusSchool.Domain.Extensions;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using Microsoft.Extensions.Configuration;
using BinusSchool.Data.Configurations;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class DetailHistoryEventHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ICertificateTemplate _serviceCertificateTemplate;

        public DetailHistoryEventHandler(
            ISchedulingDbContext userDbContext,
            IConfiguration configuration,
            ICertificateTemplate serviceCertificateTemplate
            )
        {
            _dbContext = userDbContext;
            _configuration = configuration;
            _serviceCertificateTemplate = serviceCertificateTemplate;
        }

        private string GetSignature1Name(MsCertificateTemplate msCertificateTemplate)
        {
            if (msCertificateTemplate.Signature1 == null) return "-";
            return msCertificateTemplate.User1.DisplayName;
        }

        private string GetSignature2Name(MsCertificateTemplate msCertificateTemplate)
        {
            if (msCertificateTemplate.Signature2 == null) return "-";
            return msCertificateTemplate.User2.DisplayName;
        }

        private string GetHsCertificateTemplate(MsCertificateTemplate msCertificateTemplate)
        {
            if (msCertificateTemplate.HistoryCertificateTemplateApprovers == null) return null;
            return msCertificateTemplate.HistoryCertificateTemplateApprovers.FirstOrDefault().Reason;
        }

        private string GetDescriptionApprovalEventSetting(MsCertificateTemplate msCertificateTemplate)
        {
            if (msCertificateTemplate.Signature1 == null) return "-";
            return msCertificateTemplate.User1.DisplayName;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<DetailEventSettingRequest>(nameof(DetailEventSettingRequest.Id), nameof(DetailEventSettingRequest.IdUser));

            var predicate = PredicateBuilder.Create<HTrEvent>(x => true);

            var checkDataEvent = await _dbContext.Entity<HTrEvent>().Where(x => x.Id == param.Id).FirstOrDefaultAsync(CancellationToken);

            if (checkDataEvent == null)
                throw new BadRequestException($"Data Event not found");

            var query = _dbContext.Entity<HTrEvent>()
                .Include(x => x.EventChange)
                .Include(x => x.EventType)
                .Include(x => x.AcademicYear)
                .Include(x => x.EventDetails)
                .Include(x => x.EventIntendedFor)
                .Include(x => x.EventBudgets)
                .Include(x => x.EventAttachments)
                .Include(x => x.EventActivities)
                    .ThenInclude(x => x.Activity)
                .Include(x => x.EventActivities)
                    .ThenInclude(x => x.EventActivityPICs)
                        .ThenInclude(x => x.User)
                .Include(x => x.EventActivities)
                    .ThenInclude(x => x.EventActivityRegistrants)
                        .ThenInclude(x => x.User)
                .Include(x => x.EventActivities)
                    .ThenInclude(x => x.EventActivityAwards)
                        .ThenInclude(x => x.Award)
                    .ThenInclude(x => x.EventActivityAwards)
                        .ThenInclude(x => x.HomeroomStudent)
                            .ThenInclude(x => x.Student)
                .Include(x => x.EventApprovers)
                    .ThenInclude(x => x.User)
                // .Include(x => x.EventApprovals)
                //     .ThenInclude(x => x.User)
                .Include(x => x.EventAwardApprovers)
                    .ThenInclude(x => x.User)
                .Include(x => x.EventCoordinators)
                    .ThenInclude(x => x.User)
                .Where(predicate).Where(x => x.Id == param.Id);

            var trEvent = await query
                    .SingleOrDefaultAsync(CancellationToken);

            var dataStudent = _dbContext.Entity<MsStudent>()
                                .Include(x => x.StudentGrades)
                                    .ThenInclude(x => x.Grade)
                                .Include(x => x.HomeroomStudents)
                                    .ThenInclude(x => x.Homeroom)
                                        .ThenInclude(x => x.GradePathwayClassroom)
                                            .ThenInclude(x => x.Classroom)
                                ;

            var dataUser = _dbContext.Entity<MsUser>()
                            .Include(x => x.UserRoles)
                                .ThenInclude(x => x.Role);

            var result = new DetailHistoryEventResult
            {
                Id = trEvent.Id,
                ChangeDates = trEvent.EventChange.DateIn,
                ChangeNotes = trEvent.EventChange.ChangeNotes,
                EventName = trEvent.Name,
                AcademicYear = new CodeWithIdVm(trEvent.IdAcademicYear, trEvent.AcademicYear.Code, trEvent.AcademicYear.Description),
                Dates = trEvent.EventDetails.OrderBy(y => y.StartDate).Select(y => new DateTimeRange
                {
                    Start = y.StartDate,
                    End = y.EndDate
                }),
                EventType = new CalendarEventTypeVm
                {
                    Id = trEvent.IdEventType,
                    Code = trEvent.EventType.Code,
                    Description = trEvent.EventType.Description,
                    Color = trEvent.EventType.Color
                },
                IsShowOnCalendarAcademic = trEvent.IsShowOnCalendarAcademic,
                IsShowOnSchedule = trEvent.IsShowOnSchedule,
                EventObjective = trEvent.Objective,
                EventPlace = trEvent.Place,
                EventCoordinator = new CodeWithIdVm(trEvent.EventCoordinators.First().Id, trEvent.EventCoordinators.First().IdUser, trEvent.EventCoordinators.First().User.DisplayName),
                IdUserCoordinator = trEvent.EventCoordinators.First().User.Id,
                EventLavel = trEvent.EventLevel,
                Budget = trEvent.EventBudgets != null ? trEvent.EventBudgets.Select(y => new SchoolEventBudget
                {
                    IdBudget = y.Id,
                    Name = y.Name,
                    Amount = y.Amount
                }).ToList() : null,
                AttachmentBudget = trEvent.EventAttachments != null ? trEvent.EventAttachments.Select(y => new SchoolEventAttachment
                {
                    IdAttachmant = y.Id,
                    Url = y.Url,
                    Filename = y.Filename,
                    Filetype = y.Filetype,
                    Filesize = y.Filesize
                }).ToList() : null,
                Role = new CodeWithIdVm(null, trEvent.EventIntendedFor.First().IntendedFor),
                Option = trEvent.EventIntendedFor.First().Option,
                Activity = trEvent.EventActivities != null ? trEvent.EventActivities.Select(y => new EventActivity
                {
                    Id = y.Id,
                    IdActivity = y.IdActivity,
                    NameActivity = y.Activity.Description,
                    DataActivity = new CodeWithIdVm(y.Activity.Id, y.Activity.Code, y.Activity.Description),
                    EventActivityPICIdUser = y.EventActivityPICs != null ? y.EventActivityPICs.Select(z => new UserActivity { Id = z.IdUser, DisplayName = z.User.DisplayName }).ToList() : null,
                    EventActivityRegistrantIdUser = y.EventActivityRegistrants != null ? y.EventActivityRegistrants.Select(z => new UserActivity { Id = z.User.Id, DisplayName = z.User.DisplayName }).ToList() : null,
                    EventActivityAwardIdUser = y.EventActivityAwards != null ? y.EventActivityAwards.GroupBy(z => z.IdHomeroomStudent).Select(z => new EventActivityAwardDetail
                    {
                        IdHomeroomStudent = z.Key,
                        DataStudent = dataStudent.Select(s => new DataStudent
                        {
                            Id = s.Id,
                            Fullname = s.FirstName + " " + s.LastName,
                            BinusianID = s.Id,
                            Grade = s.StudentGrades.First().Grade.Description,
                            IdHomeroomStudent = s.HomeroomStudents.First().Id,
                            Homeroom = s.StudentGrades.First().Grade.Code + s.HomeroomStudents.First().Homeroom.GradePathwayClassroom.Classroom.Description
                        }).Where(x => x.IdHomeroomStudent == z.Key).FirstOrDefault(),
                        DataAward = z.Select(a => new DataAward
                        {
                            Id = a.Award.Id,
                            Code = a.Award.Code,
                            Description = a.Award.Description,
                            Url = a.Url,
                            Filename = a.Filename,
                            Filetype = a.Filetype,
                            Filesize = a.Filesize
                        }).ToList()
                    }).ToList() : null
                }).ToList() : null,
                Approver1 = trEvent.EventApprovers.Select(y => new CodeWithIdVm
                {
                    Id = y.Id,
                    Code = y.User.Id,
                    Description = y.User.DisplayName
                }).FirstOrDefault(),
                Approver2 = trEvent.EventApprovers.Count() > 1 ? trEvent.EventApprovers.Select(y => new CodeWithIdVm
                {
                    Id = y.Id,
                    Code = y.User.Id,
                    Description = y.User.DisplayName
                }).LastOrDefault() : null,
                AwardApprover1 = trEvent.EventAwardApprovers.Select(y => new CodeWithIdVm
                {
                    Id = y.Id,
                    Code = y.User.Id,
                    Description = y.User.DisplayName
                }).FirstOrDefault(),
                AwardApprover2 = trEvent.EventAwardApprovers.Count() > 1 ? trEvent.EventAwardApprovers.Select(y => new CodeWithIdVm
                {
                    Id = y.Id,
                    Code = y.User.Id,
                    Description = y.User.DisplayName
                }).LastOrDefault() : null,
                CertificateTemplate = (await _serviceCertificateTemplate.GetCertificateTemplateDetail(new Data.Model.Scheduling.FnSchedule.CertificateTemplate.DetailCertificateTemplateRequest
                {
                    UserId = param.IdUser,
                    IdCertificateTemplate = trEvent.IdCertificateTemplate
                })).Payload,
                MandatoryType = EventIntendedForAttendanceStudent.Mandatory,
                IsAttendanceRepeat = false,

                StatusEvent = trEvent.StatusEvent,
                // StatusDeclined = trEvent.StatusEvent == "Declined" ? trEvent.EventApprovals.Where(x => !x.IsApproved).Select(x => new Declained
                // {
                //     ApprovalCount = trEvent.EventApprovals.Count(y => y.IsApproved) + 1,
                //     DeclinedBy = x.User.DisplayName,
                //     DeclinedDate = x.DateIn,
                //     Note = x.Reason
                // }).FirstOrDefault() : null,
                DescriptionEvent = trEvent.DescriptionEvent,
                StatusEventAward = trEvent.StatusEventAward,
                DescriptionEventAward = trEvent.DescriptionEventAward,
                CanLinkToMerit = trEvent.StatusEvent == "Approved" ? true : false,
                CanApprove = (trEvent.StatusEvent != "Declined" || trEvent.StatusEvent != "Approved") ? true : false,
                CanEdit = (trEvent.StatusEvent == "Declined") ? true : false,
                CanDelete = (trEvent.StatusEvent == "Approved" || trEvent.StatusEvent == "Declined") ? true : false,
                DescriptionApprovalEventSetting = trEvent.StatusEvent == "Approved" ? "" : "",
            };

            // get intended for
            if (result.Role.Code == RoleConstant.Teacher || result.Role.Code == RoleConstant.Staff)
            {
                var forTeacher = await _dbContext.Entity<HTrEventIntendedFor>()
                    .Include(y => y.EventIntendedForDepartments).ThenInclude(y => y.Department)
                    .Include(y => y.EventIntendedForPositions).ThenInclude(y => y.TeacherPosition).ThenInclude(y => y.Position)
                    .Include(y => y.EventIntendedForPersonals).ThenInclude(y => y.User)
                    .Where(y => y.IdEvent == result.Id)
                    .ToListAsync(CancellationToken);

                result.ForTeacher = new DetailForTeacher();
                    result.IntendedFor = forTeacher.Select(y => new DetailEventIntendedFor
                    {
                        IdIntendedFor = y.Id,
                        Role = y.IntendedFor,
                        Option = y.Option,
                        SendNotificationToLevelHead = y.SendNotificationToLevelHead,
                        NeedParentPermission = y.NeedParentPermission,
                        NoteToParent = y.NoteToParent,
                        IntendedForDepartemetIdDepartemet = y.EventIntendedForDepartments != null ? y.EventIntendedForDepartments.Select(d => new CodeWithIdVm
                        {
                            Id = d.IdDepartment,
                            Code = d.IdDepartment,
                            Description = d.Department.Description
                        }).ToList() : null,
                        IntendedForPositionIdTeacherPosition = y.EventIntendedForPositions != null ? y.EventIntendedForPositions.Select(p => new CodeWithIdVm
                        {
                            Id = p.IdHTrEventIntendedForPosition,
                            Code = p.IdTeacherPosition,
                            Description = p.TeacherPosition.Position.Description
                        }).ToList() : null,
                        IntendedForPersonalIdUser = y.EventIntendedForPersonals != null ? y.EventIntendedForPersonals.Select(p => new CodeWithIdVm
                        {
                            Id = p.Id,
                            Code = p.IdUser,
                            Description = p.User.DisplayName
                        }).ToList() : null
                    }).ToList();
            }
            else if (result.Role.Code == RoleConstant.Student)
            {
                var forStudent = await _dbContext.Entity<HTrEventIntendedFor>()
                        .Include(y => y.EventIntendedForLevelStudents).ThenInclude(y => y.Level)
                        .Include(y => y.EventIntendedForPersonalStudents).ThenInclude(y => y.Student).ThenInclude(y => y.HomeroomStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.Grade).ThenInclude(y => y.Level)
                        .Include(y => y.EventIntendedForPersonalStudents).ThenInclude(y => y.Student).ThenInclude(y => y.HomeroomStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.GradePathwayClassroom).ThenInclude(y => y.Classroom)
                        .Include(y => y.EventIntendedForGradeStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.Grade).ThenInclude(y => y.Level)
                        .Include(y => y.EventIntendedForGradeStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.GradePathwayClassroom).ThenInclude(y => y.Classroom)
                        .Include(y => y.EventIntendedForGradeStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.HomeroomPathways)
                        .Include(y => y.EventIntendedForPersonalParents).ThenInclude(y => y.Parent).ThenInclude(y => y.StudentParents).ThenInclude(y => y.Student).ThenInclude(y => y.HomeroomStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.Grade).ThenInclude(y => y.Level)
                        .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.HTrEventIntendedForAtdPICStudent).ThenInclude(x => x.User)
                        .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EvIntendedForAtdCheckStudent)
                        .Where(y => y.IdEvent == result.Id)
                    .ToListAsync(CancellationToken);

                var forStudentCheck = await _dbContext.Entity<HTrEventIntendedFor>()
                        .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.HTrEventIntendedForAtdPICStudent).ThenInclude(x => x.User)
                        .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EvIntendedForAtdCheckStudent)
                        .FirstOrDefaultAsync(x => x.IdEvent == result.Id,CancellationToken);
                result.ForStudent = new DetailForStudent();
                    result.IntendedFor = trEvent.EventIntendedFor.Select(y => new DetailEventIntendedFor
                    {
                        IdIntendedFor = y.Id,
                        Role = y.IntendedFor,
                        Option = y.Option,
                        SendNotificationToLevelHead = y.SendNotificationToLevelHead,
                        NeedParentPermission = y.NeedParentPermission,
                        NoteToParent = y.NoteToParent,
                        IntendedForLevelStudentIdLevel = y.EventIntendedForLevelStudents != null ? y.EventIntendedForLevelStudents.Select(ls => new CodeWithIdVm
                        {
                            Id = ls.IdLevel,
                            Code = ls.IdLevel,
                            Description = ls.Level.Description
                        }).ToList() : null,
                        IntendedForPersonalIdStudent = y.EventIntendedForPersonalStudents != null ? y.EventIntendedForPersonalStudents.Select(ps => new IntendedForStudent
                        {
                            Id = ps.Id,
                            Code = ps.IdStudent,
                            Description = ps.Student.FirstName + " " + ps.Student.LastName,
                            Username = dataUser.Where(x => x.Id == ps.IdStudent).First() != null ? dataUser.Where(x => x.Id == ps.IdStudent).First().Username : null,
                            BinusianID = ps.Student.Id,
                            Role = dataUser.Where(x => x.Id == ps.IdStudent).First() != null ? new CodeWithIdVm
                            {
                                Id = dataUser.Where(x => x.Id == ps.IdStudent).First().UserRoles.First().Role.Id,
                                Code = dataUser.Where(x => x.Id == ps.IdStudent).First().UserRoles.First().Role.Code,
                                Description = dataUser.Where(x => x.Id == ps.IdStudent).First().UserRoles.First().Role.Description
                            } : null,
                            Student = new CodeWithIdVm
                            {
                                Id = ps.Id,
                                Code = ps.IdStudent,
                                Description = ps.Student.FirstName + " " + ps.Student.LastName
                            },
                            Level = ps.Student.HomeroomStudents != null ? ps.Student.HomeroomStudents.Select(x => new CodeWithIdVm
                            {
                                Id = x.Homeroom.Grade.Level.Id,
                                Code = x.Homeroom.Grade.Level.Code,
                                Description = x.Homeroom.Grade.Level.Description,
                            }).FirstOrDefault() : null,
                            Grade = ps.Student.HomeroomStudents != null ? ps.Student.HomeroomStudents.Select(x => new CodeWithIdVm
                            {
                                Id = x.Homeroom.Grade.Id,
                                Code = x.Homeroom.Grade.Code,
                                Description = x.Homeroom.Grade.Description,
                            }).FirstOrDefault() : null,
                            Homeroom = ps.Student.HomeroomStudents != null ? ps.Student.HomeroomStudents.Select(x => new CodeWithIdVm
                            {
                                Id = x.Homeroom.Id,
                                Code = x.Homeroom.Id,
                                Description = x.Homeroom.Grade.Code + x.Homeroom.GradePathwayClassroom.Classroom.Code,
                            }).FirstOrDefault() : null
                        }).ToList() : null,
                        IntendedForGradeStudentIdHomeroomPathway = y.EventIntendedForGradeStudents != null ? y.EventIntendedForGradeStudents.Select(gs => new IntendedForHomeroomPathway
                        {
                            Id = gs.Id,
                            Code = gs.Homeroom.HomeroomPathways.First().Id,
                            Level = new CodeWithIdVm
                            {
                                Id = gs.Homeroom.Grade.Level.Id,
                                Code = gs.Homeroom.Grade.Level.Code,
                                Description = gs.Homeroom.Grade.Level.Description,
                            },
                            Grade = new CodeWithIdVm
                            {
                                Id = gs.Homeroom.Grade.Id,
                                Code = gs.Homeroom.Grade.Code,
                                Description = gs.Homeroom.Grade.Description,
                            },
                            Homeroom = new CodeWithIdVm
                            {
                                Id = gs.Homeroom.Id,
                                Code = gs.Homeroom.Id,
                                Description = gs.Homeroom.Grade.Code + gs.Homeroom.GradePathwayClassroom.Classroom.Code,
                            },

                        }).ToList() : null
                    }).ToList();
                if (forStudent != null)
                {

                    var attStudent = forStudentCheck.EventIntendedForAttendanceStudents.First();
                    result.ForStudent.AttendanceOption = attStudent.Type;
                    result.ForStudent.SetAttendanceEntry = attStudent.IsSetAttendance;

                    // use temporary show calendar academic
                    result.ForStudent.ShowOnCalendarAcademic = forStudentCheck.Event.IsShowOnCalendarAcademic;

                    if (result.ForStudent.SetAttendanceEntry)
                    {
                        var attPic = attStudent.HTrEventIntendedForAtdPICStudent.First();
                        result.ForStudent.PicAttendance = attPic.Type;

                        if (new[] { EventIntendedForAttendancePICStudent.UserStaff, EventIntendedForAttendancePICStudent.UserTeacher }.Contains(attPic.Type))
                            result.ForStudent.UserPic = new NameValueVm(attPic.IdUser, attPic.User.DisplayName);

                        result.ForStudent.RepeatAttendanceCheck = attStudent.IsRepeat;
                        result.ForStudent.AttendanceCheckDates = attStudent.EvIntendedForAtdCheckStudent
                            //.GroupBy(x => (x.StartDate, x.EndDate))
                            .GroupBy(x => (x.StartDate))
                            .OrderBy(x => x.Key)
                            .Select(x => new ForStudentAttendanceDate
                            {
                                //Date = new DateTimeRange(x.Key.StartDate, x.Key.EndDate),
                                StartDate = x.Key.Date,
                                AttendanceChecks = x.OrderBy(y => y.Time).Select(y => new ForStudentAttendanceCheck
                                {
                                    Name = y.CheckName,
                                    TimeInMinute = (int)y.Time.TotalMinutes,
                                    IsMandatory = y.IsPrimary
                                })
                            });
                    }
                }
            }
            else if (result.Role.Code == RoleConstant.Parent)
            {
                var forParent = await _dbContext.Entity<HTrEventIntendedFor>()
                               .Include(y => y.EventIntendedForPersonalParents).ThenInclude(y => y.Parent).ThenInclude(y => y.StudentParents).ThenInclude(y => y.Student).ThenInclude(y => y.HomeroomStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.GradePathwayClassroom).ThenInclude(y => y.Classroom)
                               .Include(y => y.EventIntendedForPersonalParents).ThenInclude(y => y.Parent).ThenInclude(y => y.StudentParents).ThenInclude(y => y.Student).ThenInclude(y => y.HomeroomStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.Grade).ThenInclude(y => y.Level)
                               .Include(y => y.EventIntendedForGradeParents).ThenInclude(y => y.Level)
                               .Include(y => y.EventIntendedForGradeParents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.Grade).ThenInclude(y => y.Level)
                               .Include(y => y.EventIntendedForGradeParents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.GradePathwayClassroom).ThenInclude(y => y.Classroom)
                               .Include(y => y.EventIntendedForGradeParents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.HomeroomPathways)
                    .FirstOrDefaultAsync(x => x.IdEvent == result.Id, CancellationToken);
                    result.IntendedFor = trEvent.EventIntendedFor.Select(y => new DetailEventIntendedFor
                    {
                        IdIntendedFor = y.Id,
                        Role = y.IntendedFor,
                        Option = y.Option,
                        SendNotificationToLevelHead = y.SendNotificationToLevelHead,
                        NeedParentPermission = y.NeedParentPermission,
                        NoteToParent = y.NoteToParent,
                        IntendedForPersonalIdParent = y.EventIntendedForPersonalParents != null ? y.EventIntendedForPersonalParents.Select(pp => new IntendedForParent
                        {
                            Id = pp.Id,
                            Code = pp.IdParent,
                            Description = pp.Parent.FirstName + " " + pp.Parent.LastName,
                            Student = pp.Parent.StudentParents.Select(x => new CodeWithIdVm
                            {
                                Id = x.Id,
                                Code = x.IdStudent,
                                Description = x.Student.FirstName + " " + x.Student.LastName
                            }).FirstOrDefault(),
                            Level = pp.Parent.StudentParents.Select(x => new CodeWithIdVm
                            {
                                Id = x.Student.HomeroomStudents.FirstOrDefault().Homeroom.Grade.Level.Id,
                                Code = x.Student.HomeroomStudents.FirstOrDefault().Homeroom.Grade.Level.Code,
                                Description = x.Student.HomeroomStudents.FirstOrDefault().Homeroom.Grade.Level.Description,
                            }).FirstOrDefault(),
                            Grade = pp.Parent.StudentParents.Select(x => new CodeWithIdVm
                            {
                                Id = x.Student.HomeroomStudents.FirstOrDefault().Homeroom.Grade.Id,
                                Code = x.Student.HomeroomStudents.FirstOrDefault().Homeroom.Grade.Code,
                                Description = x.Student.HomeroomStudents.FirstOrDefault().Homeroom.Grade.Description,
                            }).FirstOrDefault(),
                            Homeroom = pp.Parent.StudentParents.Select(x => new CodeWithIdVm
                            {
                                Id = x.Student.HomeroomStudents.FirstOrDefault().Homeroom.Id,
                                Code = x.Student.HomeroomStudents.FirstOrDefault().Homeroom.Id,
                                Description = x.Student.HomeroomStudents.FirstOrDefault().Homeroom.Grade.Code + x.Student.HomeroomStudents.FirstOrDefault().Homeroom.GradePathwayClassroom.Classroom.Code,
                            }).FirstOrDefault()
                        }).ToList() : null,
                        IntendedForLevelParentIdLevel = y.Option=="Level"
                                                        ? y.EventIntendedForGradeParents !=null
                                                            ? y.EventIntendedForGradeParents
                                                                .GroupBy(g => new
                                                                {
                                                                    IdLevel = g.Level.Id,
                                                                    Level = g.Level.Description,
                                                                    LevelCode = g.Level.Code
                                                                })
                                                                .Select(g => new CodeWithIdVm
                                                                {
                                                                    Id = g.Key.IdLevel,
                                                                    Description = g.Key.Level,
                                                                    Code = g.Key.LevelCode
                                                                }).ToList() 
                                                            : null 
                                                        : null,

                        IntendedForGradeParentIdHomeroomPathway = y.Option == "Grade"
                                                        ? y.EventIntendedForGradeParents != null
                                                            ? y.EventIntendedForGradeParents
                                                                .Select(g => new IntendedForHomeroomPathway
                                                                {
                                                                    Id = g.Id,
                                                                    Code = g.Homeroom.HomeroomPathways.First().Id,
                                                                    Level = new CodeWithIdVm
                                                                    {
                                                                        Id = g.Homeroom.Grade.Level.Id,
                                                                        Code = g.Homeroom.Grade.Level.Code,
                                                                        Description = g.Homeroom.Grade.Level.Description,
                                                                    },
                                                                    Grade = new CodeWithIdVm
                                                                    {
                                                                        Id = g.Homeroom.Grade.Id,
                                                                        Code = g.Homeroom.Grade.Code,
                                                                        Description = g.Homeroom.Grade.Description,
                                                                    },
                                                                    Homeroom = new CodeWithIdVm
                                                                    {
                                                                        Id = g.Homeroom.Id,
                                                                        Code = g.Homeroom.Id,
                                                                        Description = g.Homeroom.Grade.Code + g.Homeroom.GradePathwayClassroom.Classroom.Code,
                                                                    },
                                                                }).ToList()
                                                            : null
                                                        : null,

                    }).ToList();
            }
            else{
                var forParent = await _dbContext.Entity<HTrEventIntendedFor>()
                .If(result.Option == "Personal", x => x
                    .Include(y => y.EventIntendedForPersonalParents).ThenInclude(y => y.Parent).ThenInclude(y => y.StudentParents).ThenInclude(y => y.Student).ThenInclude(y => y.HomeroomStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.GradePathwayClassroom).ThenInclude(y => y.Classroom)
                    .Include(y => y.EventIntendedForPersonalParents).ThenInclude(y => y.Parent).ThenInclude(y => y.StudentParents).ThenInclude(y => y.Student).ThenInclude(y => y.HomeroomStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.Grade).ThenInclude(y => y.Level))
                    .FirstOrDefaultAsync(x => x.IdEvent == result.Id, CancellationToken);
                    result.IntendedFor = trEvent.EventIntendedFor.Select(y => new DetailEventIntendedFor
                    {
                        IdIntendedFor = y.Id,
                        Role = y.IntendedFor,
                        Option = y.Option,
                        SendNotificationToLevelHead = y.SendNotificationToLevelHead,
                        NeedParentPermission = y.NeedParentPermission,
                        NoteToParent = y.NoteToParent
                    }).ToList();
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}
