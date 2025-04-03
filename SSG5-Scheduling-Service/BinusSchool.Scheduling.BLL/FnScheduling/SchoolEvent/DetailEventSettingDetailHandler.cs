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
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Constants;
using BinusSchool.Domain.Extensions;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using Microsoft.Extensions.Configuration;
using BinusSchool.Data.Configurations;
using BinusSchool.Scheduling.FnSchedule.CertificateTemplate;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using Microsoft.Azure.Cosmos.Linq;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class DetailEventSettingDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _dateTime;
        private readonly DetailCertificateTemplateHandler _certificateHandler;

        public DetailEventSettingDetailHandler(
            ISchedulingDbContext schedulingDbContext,
            IConfiguration configuration,
            IMachineDateTime dateTime,
            DetailCertificateTemplateHandler certificateHandler)
        {
            _dbContext = schedulingDbContext;
            _configuration = configuration;
            _dateTime = dateTime;
            _certificateHandler = certificateHandler;
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

        private DataStudent GetDataStudent(string IdHomeroomStudent)
        {
            if (IdHomeroomStudent == null)
            {
                return null;
            }
            else
            {
                var dataStudent = _dbContext.Entity<MsHomeroomStudent>()
                                    .Include(X => X.Student)
                                        .ThenInclude(x => x.StudentGrades)
                                            .ThenInclude(x => x.Grade)
                                    .Include(x => x.Homeroom)
                                        .ThenInclude(x => x.GradePathwayClassroom)
                                            .ThenInclude(x => x.Classroom)
                                    .Select(x => new DataStudent
                                    {
                                        Id = x.Student.Id,
                                        Fullname = x.Student.FirstName + " " + x.Student.LastName,
                                        BinusianID = x.Student.Id,
                                        Grade = x.Student.StudentGrades.First().Grade.Description,
                                        IdHomeroomStudent = x.Id,
                                        Homeroom = x.Student.StudentGrades.First().Grade.Code + x.Student.HomeroomStudents.First().Homeroom.GradePathwayClassroom.Classroom.Description
                                    })
                                    .Where(x => x.IdHomeroomStudent == IdHomeroomStudent)
                                    .FirstOrDefault();

                return dataStudent;
            }
        }

        private DataStaff GetDataStaff(string IdStaff)
        {
            if (IdStaff == null)
            {
                return null;
            }
            else
            {
                var dataStaff = _dbContext.Entity<MsStaff>()
                                    .Select(x => new DataStaff
                                    {
                                        Id = x.IdBinusian,
                                        Fullname = x.FirstName + " " + x.LastName,
                                        BinusianID = x.IdBinusian
                                    })
                                    .Where(x => x.Id == IdStaff)
                                    .FirstOrDefault();

                return dataStaff;
            }
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<DetailEventSettingRequest>(nameof(DetailEventSettingRequest.Id), nameof(DetailEventSettingRequest.IdUser));

            var apiConfig = _configuration.GetSection("BinusSchoolService").Get<BinusSchoolApiConfiguration2>();

            var predicate = PredicateBuilder.Create<TrEvent>(x => true);

            var checkDataEvent = await _dbContext.Entity<TrEvent>().Where(x => x.Id == param.Id).FirstOrDefaultAsync(CancellationToken);

            if (checkDataEvent == null)
                throw new BadRequestException($"Data Event not found");

            var currentAY = await _dbContext.Entity<MsPeriod>()
               .Include(x => x.Grade)
                   .ThenInclude(x => x.Level)
                       .ThenInclude(x => x.AcademicYear)
               .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool)
               .Where(x => _dateTime.ServerTime.Date >= x.StartDate.Date)
               .Where(x => _dateTime.ServerTime.Date <= x.EndDate.Date)
               .Select(x => new
               {
                   Id = x.Grade.Level.AcademicYear.Id
               }).FirstOrDefaultAsync();

            var query = _dbContext.Entity<TrEvent>()
                .Include(x => x.EventType)
                .Include(x => x.AcademicYear)
                .Include(x => x.EventDetails)
                .Include(x => x.EventIntendedFor)
                .Include(x => x.EventBudgets)
                .Include(x => x.EventAttachments)
                .Include(x => x.EventActivities)
                    .ThenInclude(x => x.Activity)
                    .ThenInclude(x => x.EventActivities).ThenInclude(x => x.EventActivityPICs).ThenInclude(x => x.User)
                    .ThenInclude(x => x.EventActivityRegistrants)
                .Include(x => x.EventActivities)
                    .ThenInclude(x => x.EventActivityRegistrants).ThenInclude(x => x.User)
                .Include(x => x.EventActivities)
                    .ThenInclude(x => x.EventActivityAwardTeachers)
                        .ThenInclude(x => x.Award)
                .Include(x => x.EventActivities)
                    .ThenInclude(x => x.EventActivityAwards).ThenInclude(x => x.Award)
                .Include(x => x.EventActivities)
                    .ThenInclude(x => x.EventActivityAwards).ThenInclude(x => x.HomeroomStudent).ThenInclude(x => x.Student)
                .Include(x => x.EventApprovers)
                    .ThenInclude(x => x.User)
                .Include(x => x.EventApprovals)
                    .ThenInclude(x => x.User)
                .Include(x => x.EventAwardApprovers)
                    .ThenInclude(x => x.User)
                .Include(x => x.EventCoordinators)
                    .ThenInclude(x => x.User)
                .Where(predicate).Where(x => x.Id == param.Id);

            var trEvent = await query
                    .SingleOrDefaultAsync(CancellationToken);

            //List<string> idEventActivityRegistrants = new List<string>();
            //foreach (var item in trEvent.EventActivities)
            //{
            //    if(item.EventActivityRegistrants!=null)
            //        idEventActivityRegistrants.Add(item.Id);
            //}

            //var eventActivityRegistrants = await _dbContext.Entity<TrEventActivityRegistrant>()
            //                            .Include(e=>e.User)
            //                            .Where(e=> idEventActivityRegistrants.Contains(e.Id))
            //                            .ToListAsync(CancellationToken);

            var result = new DetailEventSettingResult
            {
                Id = trEvent.Id,
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
                    EventActivityRegistrantIdUser = y.EventActivityRegistrants
                                                        .Select(z => new UserActivity { Id = z.User.Id, DisplayName = z.User.DisplayName })
                                                        .ToList(),
                    EventActivityAwardIdUser = y.EventActivityAwards != null ? y.EventActivityAwards.GroupBy(z => z.IdHomeroomStudent).Select(z => new EventActivityAwardDetail
                    {
                        IdHomeroomStudent = z.Key,
                        DataStudent = GetDataStudent(z.Key),
                        DataAward = z.Select(a => new DataAward
                        {
                            IdEventActivityAward = a.EventActivity.EventActivityAwards.First().Id,
                            Id = a.Award.Id,
                            Code = a.Award.Code,
                            Description = a.Award.Description,
                            Url = a.Url,
                            Filename = a.Filename,
                            Filetype = a.Filetype,
                            Filesize = a.Filesize,
                            OriginalFilename = a.OriginalFilename
                        }).ToList()
                    }).ToList() : null,
                    EventActivityAwardTeacherIdUser = y.EventActivityAwardTeachers.Count > 0 ? y.EventActivityAwardTeachers.GroupBy(z => z.IdStaff).Select(z => new EventActivityAwardTeacherDetail
                    {
                        IdStaff = z.Key,
                        DataStaff = GetDataStaff(z.Key),
                        DataAward = z.Select(a => new DataAward
                        {
                            IdEventActivityAward = a.EventActivity.EventActivityAwardTeachers.First().Id,
                            Id = a.Award.Id,
                            Code = a.Award.Code,
                            Description = a.Award.Description,
                            Url = a.Url,
                            Filename = a.Filename,
                            Filetype = a.Filetype,
                            Filesize = a.Filesize,
                            OriginalFilename = a.OriginalFilename
                        }).ToList()
                    }).ToList() : null
                }).ToList() : null,
                Approver1 = trEvent.EventApprovers.Where(x => x.OrderNumber == 1).Select(y => new CodeWithIdVm
                {
                    Id = y.Id,
                    Code = y.User.Id,
                    Description = y.User.DisplayName
                }).FirstOrDefault(),
                Approver2 = trEvent.EventApprovers.Count() > 1 ? trEvent.EventApprovers.Where(x => x.OrderNumber == 2).Select(y => new CodeWithIdVm
                {
                    Id = y.Id,
                    Code = y.User.Id,
                    Description = y.User.DisplayName
                }).FirstOrDefault() : null,
                AwardApprover1 = trEvent.EventAwardApprovers.Where(x => x.OrderNumber == 1).Select(y => new CodeWithIdVm
                {
                    Id = y.Id,
                    Code = y.User.Id,
                    Description = y.User.DisplayName
                }).FirstOrDefault(),
                AwardApprover2 = trEvent.EventAwardApprovers.Count() > 1 ? trEvent.EventAwardApprovers.Where(x => x.OrderNumber == 2).Select(y => new CodeWithIdVm
                {
                    Id = y.Id,
                    Code = y.User.Id,
                    Description = y.User.DisplayName
                }).LastOrDefault() : null,
                CertificateTemplate = !string.IsNullOrEmpty(trEvent.IdCertificateTemplate) ? await _certificateHandler.GetDetailCertificateTemplate(new Data.Model.Scheduling.FnSchedule.CertificateTemplate.DetailCertificateTemplateRequest
                {
                    UserId = param.IdUser,
                    IdCertificateTemplate = trEvent.IdCertificateTemplate
                }) : null,
                MandatoryType = EventIntendedForAttendanceStudent.Mandatory,
                IsAttendanceRepeat = false,

                StatusEvent = trEvent.StatusEvent,
                StatusDeclined = trEvent.StatusEvent == "Declined" ? trEvent.EventApprovals.Count > 0 ? trEvent.EventApprovals.Where(x => !x.IsApproved).Select(x => new Declained
                {
                    ApprovalCount = trEvent.EventApprovals.Count(y => y.IsApproved) + 1,
                    DeclinedBy = x.User.DisplayName,
                    DeclinedDate = x.DateIn,
                    Note = x.Reason
                }).OrderByDescending(x => x.DeclinedDate).FirstOrDefault() : null : null,
                StatusApproved = trEvent.StatusEvent == "Approved" ? trEvent.EventApprovals.Count > 0 ? trEvent.EventApprovals.Where(x => x.IsApproved).Select(x => new Approved
                {
                    ApprovedBy = x.User.DisplayName,
                    ApprovedDate = x.DateIn
                }).OrderBy(x => x.ApprovedDate).ToList() : null : null,
                DescriptionEvent = trEvent.DescriptionEvent,
                StatusEventAward = trEvent.StatusEventAward,
                DescriptionEventAward = trEvent.DescriptionEventAward,
                CanLinkToMerit = trEvent.StatusEvent == "Approved" ? trEvent.EventActivities.Any(y => y.EventActivityAwards != null) ? true : false : false,
                CanApprove = trEvent.StatusEvent.Contains("On Review") ? true : false,
                CanEdit = trEvent.StatusEvent.Contains("On Review") ? false : true,
                CanDelete = (trEvent.StatusEvent == "Declined" || trEvent.StatusEventAward == "Declined" || (trEvent.StatusEvent == "Approved" && trEvent.StatusEventAward == "Approved")) ? (trEvent.EventApprovers.Count > 0 ? trEvent.EventApprovers.First().IdUser == param.IdUser : trEvent.StatusEvent == "Approved") ? true : false : false,
                DescriptionApprovalEventSetting = trEvent.StatusEvent == "Approved" ? "" : "",
            };

            // get intended for
            if (result.Role.Code == RoleConstant.Teacher || result.Role.Code == RoleConstant.Staff)
            {
                var forTeacher = await _dbContext.Entity<TrEventIntendedFor>()
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
                        Id = p.Id,
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
                var roles = await _dbContext.Entity<LtRole>().Include(x => x.RoleGroup).Where(x => x.IdSchool == param.IdSchool).ToListAsync(CancellationToken);
                var forStudent = await _dbContext.Entity<TrEventIntendedFor>()
                        .Include(y => y.EventIntendedForLevelStudents).ThenInclude(y => y.Level)
                        .Include(y => y.EventIntendedForPersonalStudents).ThenInclude(y => y.Student).ThenInclude(y => y.HomeroomStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.Grade).ThenInclude(y => y.Level)
                        .Include(y => y.EventIntendedForPersonalStudents).ThenInclude(y => y.Student).ThenInclude(y => y.HomeroomStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.GradePathwayClassroom).ThenInclude(y => y.Classroom)
                        .Include(y => y.EventIntendedForGradeStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.Grade).ThenInclude(y => y.Level)
                        .Include(y => y.EventIntendedForGradeStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.GradePathwayClassroom).ThenInclude(y => y.Classroom)
                        .Include(y => y.EventIntendedForGradeStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.HomeroomPathways)
                        .Include(y => y.EventIntendedForPersonalParents).ThenInclude(y => y.Parent).ThenInclude(y => y.StudentParents).ThenInclude(y => y.Student).ThenInclude(y => y.HomeroomStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.Grade).ThenInclude(y => y.Level)
                        .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdPICStudents).ThenInclude(x => x.User)
                        .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdCheckStudents)
                        .Where(y => y.IdEvent == result.Id)
                    .ToListAsync(CancellationToken);

                var forStudentCheck = await _dbContext.Entity<TrEventIntendedFor>()
                        .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdPICStudents).ThenInclude(x => x.User)
                        .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdCheckStudents)
                        .FirstOrDefaultAsync(x => x.IdEvent == result.Id, CancellationToken);
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
                        Username = ps.Student.Id,
                        BinusianID = ps.Student.Id,
                        Role = roles.Where(x => x.RoleGroup.Code == RoleConstant.Student).First() != null ? new CodeWithIdVm
                        {
                            Id = roles.Where(x => x.RoleGroup.Code == RoleConstant.Student).First().Id,
                            Code = roles.Where(x => x.RoleGroup.Code == RoleConstant.Student).First().RoleGroup.Code,
                            Description = roles.Where(x => x.RoleGroup.Code == RoleConstant.Student).First().Description
                        } : null,
                        Student = new CodeWithIdVm
                        {
                            Id = ps.Id,
                            Code = ps.IdStudent,
                            Description = ps.Student.FirstName + " " + ps.Student.LastName
                        },
                        Level = ps.Student.HomeroomStudents != null ? ps.Student.HomeroomStudents.Where(x => x.Homeroom.IdAcademicYear == currentAY.Id).Select(x => new CodeWithIdVm
                        {
                            Id = x.Homeroom.Grade.Level.Id,
                            Code = x.Homeroom.Grade.Level.Code,
                            Description = x.Homeroom.Grade.Level.Description,
                        }).FirstOrDefault() : null,
                        Grade = ps.Student.HomeroomStudents != null ? ps.Student.HomeroomStudents.Where(x => x.Homeroom.IdAcademicYear == currentAY.Id).Select(x => new CodeWithIdVm
                        {
                            Id = x.Homeroom.Grade.Id,
                            Code = x.Homeroom.Grade.Code,
                            Description = x.Homeroom.Grade.Description,
                        }).FirstOrDefault() : null,
                        Homeroom = ps.Student.HomeroomStudents != null ? ps.Student.HomeroomStudents.Where(x => x.Homeroom.IdAcademicYear == currentAY.Id).Select(x => new CodeWithIdVm
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
                        Semester = new CodeWithIdVm
                        {
                            Id = gs.Homeroom.Semester.ToString(),
                            Code = gs.Homeroom.Semester.ToString(),
                            Description = gs.Homeroom.Semester.ToString()
                        }
                    }).ToList() : null
                }).ToList();

                if (forStudent != null)
                {

                    if (forStudentCheck.EventIntendedForAttendanceStudents.Count() > 0)
                    {
                        var attStudent = forStudentCheck.EventIntendedForAttendanceStudents.First();
                        result.ForStudent.AttendanceOption = attStudent.Type;
                        result.ForStudent.SetAttendanceEntry = attStudent.IsSetAttendance;

                        // use temporary show calendar academic
                        result.ForStudent.ShowOnCalendarAcademic = forStudentCheck.Event.IsShowOnCalendarAcademic;

                        if (result.ForStudent.SetAttendanceEntry && attStudent.EventIntendedForAtdPICStudents.Count != 0)
                        {
                            var attPic = attStudent.EventIntendedForAtdPICStudents.First();
                            result.ForStudent.PicAttendance = attPic.Type;

                            if (new[] { EventIntendedForAttendancePICStudent.UserStaff, EventIntendedForAttendancePICStudent.UserTeacher }.Contains(attPic.Type))
                                result.ForStudent.UserPic = new NameValueVm(attPic.IdUser, attPic.User.DisplayName);

                            result.ForStudent.RepeatAttendanceCheck = attStudent.IsRepeat;
                            result.ForStudent.AttendanceCheckDates = attStudent.EventIntendedForAtdCheckStudents
                                //.GroupBy(x => (x.StartDate, x.EndDate))
                                .GroupBy(x => (x.StartDate))
                                .OrderBy(x => x.Key)
                                .Select(x => new ForStudentAttendanceDate
                                {
                                    //Date = new DateTimeRange(x.Key.StartDate, x.Key.EndDate),
                                    StartDate = x.Key.Date,
                                    AttendanceChecks = x.OrderBy(y => y.Time).ThenBy(y => y.CheckName).GroupBy(e => new { e.CheckName, e.Time.TotalMinutes, e.IsPrimary }).Select(y => new ForStudentAttendanceCheck
                                    {
                                        Name = y.Key.CheckName,
                                        TimeInMinute = (int)y.Key.TotalMinutes,
                                        IsMandatory = y.Key.IsPrimary
                                    })
                                });
                        }
                    }
                }
            }
            else if (result.Role.Code == RoleConstant.Parent)
            {
                var forParent = await _dbContext.Entity<TrEventIntendedFor>()
                    .Include(y => y.EventIntendedForPersonalParents).ThenInclude(y => y.Parent).ThenInclude(y => y.StudentParents).ThenInclude(y => y.Student).ThenInclude(y => y.HomeroomStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.GradePathwayClassroom).ThenInclude(y => y.Classroom)
                    .Include(y => y.EventIntendedForPersonalParents).ThenInclude(y => y.Parent).ThenInclude(y => y.StudentParents).ThenInclude(y => y.Student).ThenInclude(y => y.HomeroomStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.Grade).ThenInclude(y => y.Level)
                    .Include(y => y.EventIntendedForGradeParents).ThenInclude(y => y.Level)
                    .Include(y => y.EventIntendedForGradeParents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.Grade).ThenInclude(y => y.Level)
                    .Include(y => y.EventIntendedForGradeParents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.GradePathwayClassroom).ThenInclude(y => y.Classroom)
                    .Include(y => y.EventIntendedForGradeParents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.HomeroomPathways)
                    .Where(x => x.IdEvent == result.Id)
                    .ToListAsync(CancellationToken);

                result.IntendedFor = forParent.Select(y => new DetailEventIntendedFor
                {
                    IdIntendedFor = y.Id,
                    Role = y.IntendedFor,
                    Option = y.Option,
                    SendNotificationToLevelHead = y.SendNotificationToLevelHead,
                    NeedParentPermission = y.NeedParentPermission,
                    NoteToParent = y.NoteToParent,
                    IntendedForPersonalIdParent = y.EventIntendedForPersonalParents != null ? y.EventIntendedForPersonalParents.Select(pp => new IntendedForParent
                    {
                        Id = $"P{pp.Parent.StudentParents.Select(x => x.IdStudent).FirstOrDefault()}",
                        Code = pp.IdParent,
                        //Description = pp.Parent.FirstName + " " + pp.Parent.LastName,
                        Description = "Parent of " + pp.Parent.StudentParents.Select(x => x.Student.FirstName + " " + x.Student.LastName).FirstOrDefault(),
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
                    IntendedForLevelParentIdLevel = y.EventIntendedForGradeParents != null ? y.EventIntendedForGradeParents.Where(x => x.IdHomeroom == null).Select(ls => new CodeWithIdVm
                    {
                        Id = ls.IdLevel,
                        Code = ls.Level.Code,
                        Description = ls.Level.Description
                    }).ToList() : null,
                    IntendedForGradeParentIdHomeroomPathway = y.EventIntendedForGradeParents != null ? y.EventIntendedForGradeParents.Where(x => x.IdHomeroom != null).Select(gs => new IntendedForHomeroomPathway
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
                        Semester = new CodeWithIdVm
                        {
                            Id = gs.Homeroom.Semester.ToString(),
                            Code = gs.Homeroom.Semester.ToString(),
                            Description = gs.Homeroom.Semester.ToString()
                        }
                    }).ToList() : null
                }).ToList();

                if (result.IntendedFor.Select(x => x.IntendedForPersonalIdParent).Any())
                {
                    var intendedPersonalParents = result.IntendedFor.Select(x => x.IntendedForPersonalIdParent).FirstOrDefault().Select(x => new
                    {
                        id = x.Id,
                        code = x.Code
                    }).Distinct().ToList();

                    if (intendedPersonalParents.Any())
                    {
                        var idStudents = intendedPersonalParents.Select(x => x.id).Distinct().ToList();

                        foreach (var idStudent in idStudents)
                        {
                            result.IntendedFor.Select(x => x.IntendedForPersonalIdParent).FirstOrDefault()
                                .RemoveAll(x => x.Id == idStudent && x.Code != intendedPersonalParents.FirstOrDefault(x => x.id == idStudent).code);
                        }
                    }
                }
            }
            else
            {
                var forAll = await _dbContext.Entity<TrEventIntendedFor>()
                .If(result.Option == "ALL", x => x)
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

                var forStudent = await _dbContext.Entity<TrEventIntendedFor>()
                        .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdPICStudents).ThenInclude(x => x.User)
                        .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdCheckStudents)
                        .Where(y => y.IdEvent == result.Id)
                    .ToListAsync(CancellationToken);

                var forStudentCheck = await _dbContext.Entity<TrEventIntendedFor>()
                        .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdPICStudents).ThenInclude(x => x.User)
                        .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdCheckStudents)
                        .FirstOrDefaultAsync(x => x.IdEvent == result.Id, CancellationToken);
                result.ForAll = new DetailForAll();

                if (forStudent.Count() > 0)
                {

                    if (forStudentCheck.EventIntendedForAttendanceStudents.Count() > 0)
                    {
                        var attStudent = forStudentCheck.EventIntendedForAttendanceStudents.First();
                        result.ForAll.AttendanceOption = attStudent.Type;
                        result.ForAll.SetAttendanceEntry = attStudent.IsSetAttendance;

                        // use temporary show calendar academic
                        result.ForAll.ShowOnCalendarAcademic = forStudentCheck.Event.IsShowOnCalendarAcademic;

                        if (result.ForAll.SetAttendanceEntry && attStudent.EventIntendedForAtdPICStudents.Count != 0)
                        {
                            var attPic = attStudent.EventIntendedForAtdPICStudents.First();
                            result.ForAll.PicAttendance = attPic.Type;

                            if (new[] { EventIntendedForAttendancePICStudent.UserStaff, EventIntendedForAttendancePICStudent.UserTeacher }.Contains(attPic.Type))
                                result.ForAll.UserPic = new NameValueVm(attPic.IdUser, attPic.User.DisplayName);

                            result.ForAll.RepeatAttendanceCheck = attStudent.IsRepeat;
                            result.ForAll.AttendanceCheckDates = attStudent.EventIntendedForAtdCheckStudents
                                //.GroupBy(x => (x.StartDate, x.EndDate))
                                .GroupBy(x => (x.StartDate))
                                .OrderBy(x => x.Key)
                                .Select(x => new ForStudentAttendanceDate
                                {
                                    //Date = new DateTimeRange(x.Key.StartDate, x.Key.EndDate),
                                    StartDate = x.Key.Date,
                                    AttendanceChecks = x.OrderBy(y => y.Time).ThenBy(y => y.CheckName).GroupBy(e => new { e.CheckName, e.Time.TotalMinutes, e.IsPrimary }).Select(y => new ForStudentAttendanceCheck
                                    {
                                        Name = y.Key.CheckName,
                                        TimeInMinute = (int)y.Key.TotalMinutes,
                                        IsMandatory = y.Key.IsPrimary
                                    })
                                });
                        }
                    }
                }
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}
