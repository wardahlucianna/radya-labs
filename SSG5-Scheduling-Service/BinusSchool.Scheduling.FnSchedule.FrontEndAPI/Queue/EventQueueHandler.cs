using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using FluentEmail.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnSchedule.Queue
{
    public class EventQueueHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventQueueHandler> _logger;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IRolePosition _rolePosition;

        public EventQueueHandler(IServiceProvider serviceProvider, ILogger<EventQueueHandler> logger, ISchedulingDbContext dbContext, IMachineDateTime dateTime, IRolePosition rolePosition)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _dbContext = dbContext;
            _dateTime = dateTime;
            _rolePosition = rolePosition;
        }

        [FunctionName(nameof(EventQueue))]
        public async Task EventQueue([QueueTrigger("event-queue")] string queueItem, CancellationToken cancellationToken)
        {
            var param = JsonConvert.DeserializeObject<QueueEventRequest>(queueItem);
            _logger.LogInformation("[Queue] Event");

            var idLog = Guid.NewGuid().ToString();

            #region add Log
            var newLog = new TrLogQueueEvent
            {
                Id = idLog,
                IsProcess = true,
                StartDate = _dateTime.ServerTime,
                IdSchool = param.IdSchool
            };

            _dbContext.Entity<TrLogQueueEvent>().Add(newLog);
            await _dbContext.SaveChangesAsync();
            #endregion

            try
            {
                #region getEvent
                var idAcademicYear = await _dbContext.Entity<MsPeriod>()
                                        .Include(e => e.Grade).ThenInclude(e => e.Level)
                                        .Where(e => e.Grade.Level.AcademicYear.IdSchool == param.IdSchool
                                            && (e.StartDate.Date <= _dateTime.ServerTime.Date && e.EndDate.Date >= _dateTime.ServerTime.Date))
                                        .Select(e => e.Grade.Level.IdAcademicYear)
                                        .FirstOrDefaultAsync(cancellationToken);

                var listEvent = await _dbContext.Entity<TrEventDetail>()
                                    .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForDepartments)
                                    .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForPositions)
                                    .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForGradeStudents)
                                    .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForGradeParents)
                                    .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForLevelStudents)
                                    .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForPersonalParents).ThenInclude(e => e.Parent).ThenInclude(e => e.StudentParents)
                                    .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForPersonals)
                                    .Include(e => e.Event).ThenInclude(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForPersonalStudents)
                                    .Where(e => e.Event.AcademicYear.Id == idAcademicYear)
                                    .OrderBy(e => e.DateIn)
                                    .ToListAsync(cancellationToken);

                if (listEvent.Any())
                {
                    GetUserRolePositionRequest paramUserRole = new GetUserRolePositionRequest
                    {
                        IdAcademicYear = idAcademicYear,
                        IdSchool = param.IdSchool,
                        UserRolePositions = new List<GetUserRolePosition>(),
                    };

                    //var listUser = await _dbContext.Entity<MsUser>().ToListAsync(cancellationToken);

                    List<GetUserRolePositionResult> GetUserRolePosition = new List<GetUserRolePositionResult>();
                    List<GetUserRolePositionResult> GetUserRolePositionAll = new List<GetUserRolePositionResult>();
                    foreach (var itemEvent in listEvent)
                    {
                        var index = listEvent.IndexOf(itemEvent) + 1;
                        _logger.LogWarning($"Loading...{index}/{listEvent.Count()}");

                        var listEventFors = itemEvent.Event.EventIntendedFor.ToList();
                        paramUserRole.UserRolePositions = new List<GetUserRolePosition>();

                        foreach (var itemIntendedFor in listEventFors)
                        {
                            UserRolePersonalOptionRole role = UserRolePersonalOptionRole.ALL;

                            switch (itemIntendedFor.IntendedFor)
                            {
                                case "ALL":
                                    role = UserRolePersonalOptionRole.ALL;
                                    break;
                                case "TEACHER":
                                    role = UserRolePersonalOptionRole.TEACHER;
                                    break;
                                case "STUDENT":
                                    role = UserRolePersonalOptionRole.STUDENT;
                                    break;
                                case "STAFF":
                                    role = UserRolePersonalOptionRole.STAFF;
                                    break;
                                case "PARENT":
                                    role = UserRolePersonalOptionRole.PARENT;
                                    break;
                                default:
                                    break;
                            }

                            UserRolePersonalOptionType option = UserRolePersonalOptionType.None;
                            var listPersonal = new List<string>();
                            var listLevel = new List<string>();
                            var listHomeroom = new List<string>();
                            switch (itemIntendedFor.Option)
                            {
                                case "None":
                                    option = UserRolePersonalOptionType.None;
                                    break;
                                case "All":
                                    option = UserRolePersonalOptionType.All;
                                    break;
                                case "Grade":
                                    option = UserRolePersonalOptionType.Grade;
                                    if (itemIntendedFor.IntendedFor == "STUDENT")
                                    {
                                        var listIdHomeroom = itemIntendedFor.EventIntendedForGradeStudents
                                                                            .Select(e => e.IdHomeroom).Distinct()
                                                                            .ToList();
                                        listHomeroom.AddRange(listIdHomeroom);
                                    }
                                    else if (itemIntendedFor.IntendedFor == "PARENT")
                                    {
                                        var listIdHomeroom = itemIntendedFor.EventIntendedForGradeParents
                                                                            .Select(e => e.IdHomeroom).Distinct()
                                                                            .ToList();
                                        listHomeroom.AddRange(listIdHomeroom);
                                    }
                                    break;
                                case "Department":
                                    option = UserRolePersonalOptionType.Department;
                                    break;
                                case "Personal":
                                    option = UserRolePersonalOptionType.Personal;
                                    if (itemIntendedFor.IntendedFor == "TEACHER" || itemIntendedFor.IntendedFor == "STAFF")
                                    {
                                        var listIdUserPersonal = itemIntendedFor.EventIntendedForPersonals.Select(e => e.IdUser).Distinct().ToList();
                                        listPersonal.AddRange(listIdUserPersonal);
                                    }
                                    else if (itemIntendedFor.IntendedFor == "STUDENT")
                                    {
                                        var listIdStudent = itemIntendedFor.EventIntendedForPersonalStudents
                                                                            .Select(e => e.IdStudent).Distinct()
                                                                            .ToList();
                                        listPersonal.AddRange(listIdStudent);
                                    }
                                    else if (itemIntendedFor.IntendedFor == "PARENT")
                                    {
                                        var listIdStudent = itemIntendedFor.EventIntendedForPersonalParents
                                                                    .SelectMany(e => e.Parent.StudentParents.Select(f => "P" + f.IdStudent)).Distinct()
                                                                    .ToList();



                                        listPersonal.AddRange(listIdStudent);
                                    }
                                    break;
                                case "Position":
                                    option = UserRolePersonalOptionType.Position;
                                    break;
                                case "Level":
                                    option = UserRolePersonalOptionType.Level;
                                    if (itemIntendedFor.IntendedFor == "STUDENT")
                                    {
                                        var listIdLlevel = itemIntendedFor.EventIntendedForLevelStudents
                                                                            .Select(e => e.IdLevel).Distinct()
                                                                            .ToList();
                                        listLevel.AddRange(listIdLlevel);
                                    }
                                    else if (itemIntendedFor.IntendedFor == "PARENT")
                                    {
                                        var listIdLlevel = itemIntendedFor.EventIntendedForGradeParents
                                                                            .Select(e => e.IdLevel).Distinct()
                                                                            .ToList();
                                        listLevel.AddRange(listIdLlevel);
                                    }
                                    break;
                                default:
                                    break;
                            }

                            var intendedForPositionIdTeacherPosition = itemIntendedFor.EventIntendedForPositions.Select(e => e.IdTeacherPosition).ToList();
                            var intendedForPositionIdDepartemen = itemIntendedFor.EventIntendedForDepartments.Select(e => e.IdDepartment).ToList();

                            var newUserRolePosition = new GetUserRolePosition
                            {
                                IdUserRolePositions = itemEvent.Id,
                                Role = role,
                                Option = option,
                                TeacherPositions = intendedForPositionIdTeacherPosition,
                                Departemens = intendedForPositionIdDepartemen,
                                Level = listLevel,
                                Homeroom = listHomeroom,
                                Personal = listPersonal
                            };

                            paramUserRole.UserRolePositions.Add(newUserRolePosition);
                        }

                        if (!GetUserRolePositionAll.Any() && paramUserRole.UserRolePositions.Where(e => e.Role == UserRolePersonalOptionRole.ALL).Any())
                        {
                            var apiUserRole = await _rolePosition.GetUserRolePosition(paramUserRole);
                            GetUserRolePosition = apiUserRole.IsSuccess ? apiUserRole.Payload : null;
                            GetUserRolePositionAll = GetUserRolePosition;
                        }
                        else if (paramUserRole.UserRolePositions.Where(e => e.Role == UserRolePersonalOptionRole.ALL).Any())
                        {
                            GetUserRolePositionAll.ForEach(e => e.IdUserRolePositions = itemEvent.Id);
                            GetUserRolePosition = GetUserRolePositionAll;
                        }

                        if (paramUserRole.UserRolePositions.Where(e => e.Role != UserRolePersonalOptionRole.ALL).Any())
                        {
                            var apiUserRole = await _rolePosition.GetUserRolePosition(paramUserRole);
                            GetUserRolePosition = apiUserRole.IsSuccess ? apiUserRole.Payload : null;
                        }

                        var listIdEventDetail = GetUserRolePosition.Select(e => e.IdUserRolePositions).Distinct().ToList();

                        var listUserEvent = await _dbContext.Entity<TrUserEvent>()
                                            .Include(e => e.EventDetail)
                                            .IgnoreQueryFilters()
                                            .Where(e => listIdEventDetail.Contains(e.IdEventDetail))
                                            .ToListAsync(cancellationToken);

                        var UserEventByIdEventDetail = GetUserRolePosition
                                                    .Where(e => e.IdUserRolePositions == itemEvent.Id)
                                                    .Select(e => e.IdUser)
                                                    .Distinct().ToList();

                        if (!UserEventByIdEventDetail.Any())
                            continue;

                        var userEvent = listUserEvent.Where(e => e.IdEventDetail == itemEvent.Id).ToList();
                        var listIdUser = userEvent.Select(e => e.IdUser).ToList();

                        //non aktifkan user role
                        var removeUserEvent = userEvent.Where(e => !UserEventByIdEventDetail.Contains(e.IdUser) && e.IsActive).ToList();
                        removeUserEvent.ForEach(e => e.IsActive = false);
                        _dbContext.Entity<TrUserEvent>().UpdateRange(removeUserEvent);

                        //aktifkan user role
                        var UpdateUserEvent = userEvent
                                                .Where(e => UserEventByIdEventDetail.Contains(e.IdUser)
                                                        && !e.IsActive)
                                                .ToList();

                        UpdateUserEvent.ForEach(e => e.IsActive = true);
                        _dbContext.Entity<TrUserEvent>().UpdateRange(UpdateUserEvent);

                        //add user role
                        var listAddUserEvent = UserEventByIdEventDetail
                                .Where(e => !listIdUser.Contains(e))
                                .ToList();

                        var listUserAddEvent = await _dbContext.Entity<MsUser>()
                                           .Where(e => listAddUserEvent.Contains(e.Id))
                                           .Select(e => e.Id)
                                           .ToListAsync(cancellationToken);

                        var addUserEvent = listUserAddEvent
                                .Select(e => new TrUserEvent
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdEventDetail = itemEvent.Id,
                                    IdUser = e,
                                    IsApproved = itemEvent.Event.EventIntendedFor.Select(e => e.NeedParentPermission).FirstOrDefault() ? false : true,
                                    IsNeedApproval = itemEvent.Event.EventIntendedFor.Select(e => e.NeedParentPermission).FirstOrDefault() ? true : false,
                                })
                                .ToList();

                        _dbContext.Entity<TrUserEvent>().AddRange(addUserEvent);
                        await _dbContext.SaveChangesAsync();

                    }
                }
                #endregion

                #region Update Log Message
                var updateLog = await _dbContext.Entity<TrLogQueueEvent>()
                               .Where(e => e.Id == idLog)
                               .FirstOrDefaultAsync(cancellationToken);

                updateLog.IsDone = true;
                updateLog.IsError = false;
                updateLog.IsProcess = false;
                updateLog.EndDate = _dateTime.ServerTime;

                _dbContext.Entity<TrLogQueueEvent>().Update(updateLog);
                await _dbContext.SaveChangesAsync();
                #endregion
            }
            catch (Exception ex)
            {
                #region Update Log Message
                var updateLog = await _dbContext.Entity<TrLogQueueEvent>()
                               .Where(e => e.Id == idLog)
                               .FirstOrDefaultAsync(cancellationToken);

                updateLog.IsDone = false;
                updateLog.IsError = true;
                updateLog.IsProcess = false;
                updateLog.EndDate = _dateTime.ServerTime;
                updateLog.ErrorMessage = ex.Message;

                _dbContext.Entity<TrLogQueueEvent>().Update(updateLog);
                await _dbContext.SaveChangesAsync();
                #endregion
            }
        }
    }
}
