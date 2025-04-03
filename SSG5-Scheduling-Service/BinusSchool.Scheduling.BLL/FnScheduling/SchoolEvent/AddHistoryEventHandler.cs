using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.SchoolEvent.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class AddHistoryEventHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public AddHistoryEventHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddHistoryEventRequest, AddHistoryEventValidator>();

            var existEvent = await _dbContext.Entity<TrEvent>()
                .FirstOrDefaultAsync(x => x.Id == body.IdEvent, CancellationToken);

            if (existEvent is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Event"], "Id", body.IdEvent));

            #region Event Change
            var DisplayName = _dbContext.Entity<MsUser>()
                .SingleOrDefault(e => e.Id == body.IdUser).DisplayName;
            var idEventChange = Guid.NewGuid().ToString();
            var newEventChange = new TrEventChange
            {
                Id = idEventChange,
                IdEvent = existEvent.Id,
                ChangeNotes = body.ActionType + " by " + DisplayName
            };
            _dbContext.Entity<TrEventChange>().Add(newEventChange);
            #endregion

            #region History Event
            var newHTrEvent = new HTrEvent
            {
                Id = idEventChange,
                IdEventType = existEvent.IdEventType,
                IdAcademicYear = existEvent.IdAcademicYear,
                Name = existEvent.Name,
                IsShowOnCalendarAcademic = existEvent.IsShowOnCalendarAcademic,
                IsShowOnSchedule = existEvent.IsShowOnSchedule,
                Objective = existEvent.Objective,
                Place = existEvent.Place,
                EventLevel = existEvent.EventLevel,
                StatusEvent = existEvent.StatusEvent,
                DescriptionEvent = existEvent.DescriptionEvent,
                StatusEventAward = existEvent.StatusEventAward,
                DescriptionEventAward = existEvent.DescriptionEventAward,
                IdCertificateTemplate = (!string.IsNullOrEmpty(existEvent.IdCertificateTemplate)) ? existEvent.IdCertificateTemplate : null,
            };
            _dbContext.Entity<HTrEvent>().Add(newHTrEvent);
            #endregion


            #region History Event Detail
            var UpdateEventDetail = await _dbContext.Entity<TrEventDetail>().Where(e => e.IdEvent == existEvent.Id).ToListAsync(CancellationToken);

            foreach (var ItemDetail in UpdateEventDetail)
            {
                var newHTrEventDetail = new HTrEventDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    StartDate = ItemDetail.StartDate,
                    EndDate = ItemDetail.EndDate,
                    //StartTime = ItemDetail.StartTime,
                    //EndTime = ItemDetail.EndTime,
                };
                _dbContext.Entity<HTrEventDetail>().Add(newHTrEventDetail);
            }
            #endregion

            #region IntendedFor

            var GetEventIntendedFor = await _dbContext.Entity<TrEventIntendedFor>()
                   .Where(e => e.IdEvent == existEvent.Id)
                   .ToListAsync(CancellationToken);

            var GetEventIntendedForDepartment = await _dbContext.Entity<TrEventIntendedForDepartment>()
                .Include(e => e.EventIntendedFor)
                .Where(e => e.EventIntendedFor.IdEvent == existEvent.Id)
                .ToListAsync(CancellationToken);

            var GetEventIntendedForPosition = await _dbContext.Entity<TrEventIntendedForPosition>()
                .Include(e => e.EventIntendedFor)
                .Where(e => e.EventIntendedFor.IdEvent == existEvent.Id)
                .ToListAsync(CancellationToken);

            var GetEventIntendedForPersonal = await _dbContext.Entity<TrEventIntendedForPersonal>()
                .Include(e => e.EventIntendedFor)
                .Where(e => e.EventIntendedFor.IdEvent == existEvent.Id)
                .ToListAsync(CancellationToken);

            var GetEventIntendedForLevelStudent = await _dbContext.Entity<TrEventIntendedForLevelStudent>()
                .Include(e => e.EventIntendedFor)
                .Where(e => e.EventIntendedFor.IdEvent == existEvent.Id)
                .ToListAsync(CancellationToken);

            var GetEventIntendedForGradeStudent = await _dbContext.Entity<TrEventIntendedForGradeStudent>()
                .Include(e => e.EventIntendedFor)
                .Where(e => e.EventIntendedFor.IdEvent == existEvent.Id)
                .ToListAsync(CancellationToken);

            var GetEventIntendedForPersonalStudent = await _dbContext.Entity<TrEventIntendedForPersonalStudent>()
                .Include(e => e.EventIntendedFor)
                .Where(e => e.EventIntendedFor.IdEvent == existEvent.Id)
                .ToListAsync(CancellationToken);

            var GetEventIntendedForPersonalParent = await _dbContext.Entity<TrEventIntendedForPersonalParent>()
               .Include(e => e.EventIntendedFor)
               .Where(e => e.EventIntendedFor.IdEvent == existEvent.Id)
               .ToListAsync(CancellationToken);

            var GetEventIntendedForAttendanceStudent = await _dbContext.Entity<TrEventIntendedForAttendanceStudent>()
                .Include(e => e.EventIntendedFor)
                .Where(e => e.EventIntendedFor.IdEvent == existEvent.Id)
               .ToListAsync(CancellationToken);

            var GetEventIntendedForAttendancePIC = await _dbContext.Entity<TrEventIntendedForAtdPICStudent>()
               .Include(e => e.EventIntendedForAttendanceStudent).ThenInclude(e => e.EventIntendedFor)
               .Where(e => e.EventIntendedForAttendanceStudent.EventIntendedFor.IdEvent == existEvent.Id)
               .ToListAsync(CancellationToken);

            var GetEventIntendedForAttendanceCheck = await _dbContext.Entity<TrEventIntendedForAtdCheckStudent>()
               .Include(e => e.EventIntendedForAttendanceStudent).ThenInclude(e => e.EventIntendedFor)
               .Where(e => e.EventIntendedForAttendanceStudent.EventIntendedFor.IdEvent == existEvent.Id)
               .ToListAsync(CancellationToken);
            #endregion

            #region History IntendedFor
            foreach (var ItemIntendedFor in GetEventIntendedFor)
            {
                var newHTrEventIntededFor = new HTrEventIntendedFor
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    IntendedFor = ItemIntendedFor.IntendedFor,
                    Option = ItemIntendedFor.Option,
                    SendNotificationToLevelHead = ItemIntendedFor.SendNotificationToLevelHead,
                    NeedParentPermission = ItemIntendedFor.NeedParentPermission,
                    NoteToParent = ItemIntendedFor.NoteToParent,
                };
                _dbContext.Entity<HTrEventIntendedFor>().AddRange(newHTrEventIntededFor);

                #region History IntededForDept
                var dataIndetdedForDept = GetEventIntendedForDepartment.Where(e => e.IdEventIntendedFor == ItemIntendedFor.Id).ToList();

                foreach (var ItemIntendedForDept in dataIndetdedForDept)
                {
                    var newHTrEventIntededForDept = new HTrEventIntendedForDepartment
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventIntendedFor = newHTrEventIntededFor.Id,
                        IdDepartment = ItemIntendedForDept.IdDepartment,
                    };
                    _dbContext.Entity<HTrEventIntendedForDepartment>().Add(newHTrEventIntededForDept);
                }
                #endregion

                #region History IntededForPosition
                var dataIndetdedForPosition = GetEventIntendedForPosition.Where(e => e.IdEventIntendedFor == ItemIntendedFor.Id).ToList();

                foreach (var ItemIntendedForPositon in GetEventIntendedForPosition)
                {
                    var newHTrEventIntededForPosition = new HTrEventIntendedForPosition
                    {
                        IdHTrEventIntendedForPosition = Guid.NewGuid().ToString(),
                        IdEventIntendedFor = newHTrEventIntededFor.Id,
                        IdTeacherPosition = ItemIntendedForPositon.IdTeacherPosition,
                    };
                    _dbContext.Entity<HTrEventIntendedForPosition>().Add(newHTrEventIntededForPosition);
                }
                #endregion

                #region History IntededForPosition
                var dataIndetdedForPersonal = GetEventIntendedForPersonal.Where(e => e.IdEventIntendedFor == ItemIntendedFor.Id).ToList();

                foreach (var ItemIntendedForPersonal in dataIndetdedForPersonal)
                {
                    var newHTrEventIntededForPersonal = new HTrEventIntendedForPersonal
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventIntendedFor = newHTrEventIntededFor.Id,
                        IdUser = ItemIntendedForPersonal.IdUser,
                    };
                    _dbContext.Entity<HTrEventIntendedForPersonal>().Add(newHTrEventIntededForPersonal);
                }
                #endregion

                #region History IntendedForLevelStudent
                var dataIndetdedForLevel = GetEventIntendedForLevelStudent.Where(e => e.IdEventIntendedFor == ItemIntendedFor.Id).ToList();

                foreach (var ItemIntendedForLevel in dataIndetdedForLevel)
                {
                    var newHTrEventIntededForLevel = new HTrEventIntendedForLevelStudent
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventIntendedFor = newHTrEventIntededFor.Id,
                        IdLevel = ItemIntendedForLevel.IdLevel,
                    };
                    _dbContext.Entity<HTrEventIntendedForLevelStudent>().Add(newHTrEventIntededForLevel);
                }
                #endregion

                #region History IntendedForGradeStudent
                var dataIndetdedForGrade = GetEventIntendedForGradeStudent.Where(e => e.IdEventIntendedFor == ItemIntendedFor.Id).ToList();

                foreach (var ItemIntendedForGrade in dataIndetdedForGrade)
                {
                    var newHTrEventIntededForGrade = new HTrEventIntendedForGradeStudent
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventIntendedFor = newHTrEventIntededFor.Id,
                        IdHomeroom = ItemIntendedForGrade.IdHomeroom,
                    };
                    _dbContext.Entity<HTrEventIntendedForGradeStudent>().Add(newHTrEventIntededForGrade);
                }
                #endregion

                #region History IntendedForPersonalStudent
                var dataIndetdedForPersonalStudent = GetEventIntendedForPersonalStudent.Where(e => e.IdEventIntendedFor == ItemIntendedFor.Id).ToList();

                foreach (var ItemIntendedForPersonalStudent in dataIndetdedForPersonalStudent)
                {
                    var newHTrEventIntededForPersonalStudent = new HTrEventIntendedForPersonalStudent
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventIntendedFor = newHTrEventIntededFor.Id,
                        IdStudent = ItemIntendedForPersonalStudent.IdStudent,
                    };
                    _dbContext.Entity<HTrEventIntendedForPersonalStudent>().Add(newHTrEventIntededForPersonalStudent);
                }
                #endregion

                #region History IntendedForPersonalParent
                var dataIndetdedForPersonalPersonalParent = GetEventIntendedForPersonalParent.Where(e => e.IdEventIntendedFor == ItemIntendedFor.Id).ToList();

                foreach (var ItemIntendedForPersonalParent in dataIndetdedForPersonalPersonalParent)
                {
                    var newHTrEventIntededForPersonalParent = new HTrEventIntendedForPersonalParent
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventIntendedFor = newHTrEventIntededFor.Id,
                        IdParent = ItemIntendedForPersonalParent.IdParent,
                    };
                    _dbContext.Entity<HTrEventIntendedForPersonalParent>().Add(newHTrEventIntededForPersonalParent);
                }
                #endregion

                #region History IntendedForAttendanceStudent
                var DataIntendedForAttendanceStudent = GetEventIntendedForAttendanceStudent.Where(e => e.IdEventIntendedFor == ItemIntendedFor.Id).ToList();
                foreach (var ItemIntendedForAttendanceStudent in DataIntendedForAttendanceStudent)
                {
                    var newHTrEventIntededForAttendanceStudent = new HTrEventIntendedForAtdStudent
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventIntendedFor = newHTrEventIntededFor.Id,
                        Type = ItemIntendedForAttendanceStudent.Type,
                        IsSetAttendance = ItemIntendedForAttendanceStudent.IsSetAttendance,
                        IsRepeat = ItemIntendedForAttendanceStudent.IsRepeat,
                    };
                    _dbContext.Entity<HTrEventIntendedForAtdStudent>().Add(newHTrEventIntededForAttendanceStudent);

                    #region IntendedForAttendancePIC
                    var DataIntendedForAttendancePIC = GetEventIntendedForAttendancePIC.Where(e => e.IdEventIntendedForAttendanceStudent == ItemIntendedForAttendanceStudent.Id).ToList();
                    foreach (var ItemIntendedForAttendancePIC in DataIntendedForAttendancePIC)
                    {
                        var newHTrEventIntededForAttendancePIC = new HTrEventIntendedForAtdPICStudent
                        {
                            IdHTrEventIntendedForAtdPICStudent = Guid.NewGuid().ToString(),
                            IdEventIntendedForAttendanceStudent = newHTrEventIntededForAttendanceStudent.Id,
                            Type = ItemIntendedForAttendancePIC.Type,
                            IdUser = ItemIntendedForAttendancePIC.IdUser,
                        };
                        _dbContext.Entity<HTrEventIntendedForAtdPICStudent>().Add(newHTrEventIntededForAttendancePIC);
                    }
                    #endregion

                    #region IntendedForAttendanceCheck
                    var DataIntendedForAttendanceCheck = GetEventIntendedForAttendanceCheck.Where(e => e.IdEventIntendedForAttendanceStudent == ItemIntendedForAttendanceStudent.Id).ToList();
                    foreach (var ItemIntendedForAttendanceCheck in DataIntendedForAttendanceCheck)
                    {
                        var newHTrEventIntededForAttendanceCheck = new HTrEvIntendedForAtdCheckStudent
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdHTrEventIntendedForAtdStudent = newHTrEventIntededForAttendanceStudent.Id,
                            CheckName = ItemIntendedForAttendanceCheck.CheckName,
                            Time = ItemIntendedForAttendanceCheck.Time,
                            IsPrimary = ItemIntendedForAttendanceCheck.IsPrimary,
                            StartDate = ItemIntendedForAttendanceCheck.StartDate,
                            StartTime = ItemIntendedForAttendanceCheck.StartTime,
                            EndTime = ItemIntendedForAttendanceCheck.EndTime,
                        };
                        _dbContext.Entity<HTrEvIntendedForAtdCheckStudent>().Add(newHTrEventIntededForAttendanceCheck);
                    }
                    #endregion
                }
                #endregion
            }
            #endregion

            #region Event Coordinator
            var GetCoordinator = _dbContext.Entity<TrEventCoordinator>()
            .SingleOrDefault(e => e.IdEvent == existEvent.Id);

            #region History Coordinator
            if (GetCoordinator != null)
            {
                var newHTrEventCoordinator = new HTrEventCoordinator
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    IdUser = GetCoordinator.IdUser,
                };
                _dbContext.Entity<HTrEventCoordinator>().Add(newHTrEventCoordinator);
            }
            #endregion
            #endregion

            #region Event Budget
            var GetBudget = await _dbContext.Entity<TrEventBudget>()
                .Where(e => e.IdEvent == existEvent.Id).ToListAsync(CancellationToken);

            #region History Budget
            foreach (var ItemBudget in GetBudget)
            {
                var newHTrEventBudget = new HTrEventBudget
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    Name = ItemBudget.Name,
                    Amount = ItemBudget.Amount,
                };
                _dbContext.Entity<HTrEventBudget>().Add(newHTrEventBudget);
            }
            #endregion
            #endregion

            #region Event Attachmant
            var GetAttachmant = await _dbContext.Entity<TrEventAttachment>()
                .Where(e => e.IdEvent == existEvent.Id).ToListAsync(CancellationToken);

            #region History Attachmant
            foreach (var ItemAttachmant in GetAttachmant)
            {
                var newHTrEventAttachment = new HTrEventAttachment
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    Url = ItemAttachmant.Url,
                    Filename = ItemAttachmant.Filename,
                    Filetype = ItemAttachmant.Filetype,
                };
                _dbContext.Entity<HTrEventAttachment>().Add(newHTrEventAttachment);
            }
            #endregion
            #endregion

            #region Event Activity
            var GetActivity = await _dbContext.Entity<TrEventActivity>()
                .Include(e => e.EventActivityRegistrants)
                .Include(e => e.EventActivityPICs)
                .Include(e => e.EventActivityAwards)
                .Include(e => e.EventActivityAwardTeachers)
                .Where(e => e.IdEvent == existEvent.Id).ToListAsync(CancellationToken);

            foreach (var ItemActivity in GetActivity)
            {
                #region History Activity
                var newHTrEventActivity = new HTrEventActivity
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    IdActivity = ItemActivity.IdActivity,
                };
                _dbContext.Entity<HTrEventActivity>().Add(newHTrEventActivity);
                #endregion

                #region History PIC
                foreach (var ItemActivityPIC in ItemActivity.EventActivityPICs)
                {
                    var newHTrEventActivityPIC = new HTrEventActivityPIC
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventActivity = newHTrEventActivity.Id,
                        IdUser = ItemActivityPIC.IdUser,
                    };
                    _dbContext.Entity<HTrEventActivityPIC>().Add(newHTrEventActivityPIC);
                }
                #endregion

                #region History Regristant
                foreach (var ItemActivityRegistrants in ItemActivity.EventActivityRegistrants)
                {
                    var newHTrEventActivityRegristrant = new HTrEventActivityRegistrant
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventActivity = newHTrEventActivity.Id,
                        IdUser = ItemActivityRegistrants.IdUser,
                    };
                    _dbContext.Entity<HTrEventActivityRegistrant>().Add(newHTrEventActivityRegristrant);
                }
                #endregion

                #region History Award
                foreach (var ItemActivityAward in ItemActivity.EventActivityAwards)
                {
                    var newHTrEventActivityAward = new HTrEventActivityAward
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventActivity = newHTrEventActivity.Id,
                        IdAward = ItemActivityAward.IdAward,
                        IdHomeroomStudent = ItemActivityAward.IdHomeroomStudent,
                        Url = ItemActivityAward.Url,
                        Filename = ItemActivityAward.Filename,
                        Filetype = ItemActivityAward.Filetype,
                        Filesize = ItemActivityAward.Filesize,
                        OriginalFilename = ItemActivityAward.OriginalFilename
                    };

                    _dbContext.Entity<HTrEventActivityAward>().Add(newHTrEventActivityAward);
                    await _dbContext.SaveChangesAsync(CancellationToken);
                }
                #endregion

                #region History Award Teacher
                foreach (var ItemActivityAwardTeacher in ItemActivity.EventActivityAwardTeachers)
                {
                    var newHTrEventActivityAwardTeacher = new HTrEventActivityAwardTeacher
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventActivity = newHTrEventActivity.Id,
                        IdAward = ItemActivityAwardTeacher.IdAward,
                        IdStaff = ItemActivityAwardTeacher.IdStaff,
                        Url = ItemActivityAwardTeacher.Url,
                        Filename = ItemActivityAwardTeacher.Filename,
                        Filetype = ItemActivityAwardTeacher.Filetype,
                        Filesize = ItemActivityAwardTeacher.Filesize,
                        OriginalFilename = ItemActivityAwardTeacher.OriginalFilename
                    };

                    _dbContext.Entity<HTrEventActivityAwardTeacher>().Add(newHTrEventActivityAwardTeacher);
                    await _dbContext.SaveChangesAsync(CancellationToken);
                }
                #endregion
            }
            #endregion

            #region Approval Event
            var GetApprovalEvent = await _dbContext.Entity<TrEventApprover>()
                    .Where(e => e.IdEvent == existEvent.Id).ToListAsync(CancellationToken);

            #region History Approver Event
            foreach(var ItemApprovalEvent in GetApprovalEvent)
            {
                var newEventApprover = new HTrEventApprover
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    IdUser = ItemApprovalEvent.IdUser,
                };
                _dbContext.Entity<HTrEventApprover>().Add(newEventApprover);
            }
            #endregion
            #endregion

            #region Approval Award

            #region History Approver Event
            var GetApprovalAward = await _dbContext.Entity<TrEventAwardApprover>()
                    .Where(e => e.IdEvent == existEvent.Id).ToListAsync(CancellationToken);

            foreach (var ItemApprovalAward in GetApprovalAward)
            {
                var newAwardApprover = new HTrEventAwardApprover
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    IdUser = ItemApprovalAward.IdUser,
                };
                _dbContext.Entity<HTrEventAwardApprover>().Add(newAwardApprover);
            }
            #endregion
            #endregion
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
