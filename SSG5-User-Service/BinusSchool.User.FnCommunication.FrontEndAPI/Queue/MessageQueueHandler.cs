using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.User.FnCommunication.Message.Notification;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Common.Exceptions;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.Formula.Functions;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BinusSchool.Common.Abstractions;
using System.Linq;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Persistence.UserDb.Entities.School;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.User.FnCommunication.Queue
{
    public class MessageQueueHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MessageQueueHandler> _logger;
        private readonly IUserDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IRolePosition _rolePosition;

        public MessageQueueHandler(IServiceProvider serviceProvider, ILogger<MessageQueueHandler> logger, IUserDbContext dbContext, IMachineDateTime dateTime, IRolePosition rolePosition)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _dbContext = dbContext;
            _dateTime = dateTime;
            _rolePosition = rolePosition;
        }

        [FunctionName(nameof(MessageQueue))]
        public async Task MessageQueue([QueueTrigger("message-queue")] string queueItem, CancellationToken cancellationToken)
        {
            var param = JsonConvert.DeserializeObject<QueueMessagesRequest>(queueItem);
            _logger.LogInformation("[Queue] Message");

            var idLog = Guid.NewGuid().ToString();

            #region add Log
            var newLog = new TrLogQueueMessage
            {
                Id = idLog,
                IsProcess = true,
                StartDate = _dateTime.ServerTime,
                IdSchool = param.IdSchool
            };

            _dbContext.Entity<TrLogQueueMessage>().Add(newLog);
            await _dbContext.SaveChangesAsync();
            #endregion

            try
            {
                #region getMessage
                var listMessagge = await _dbContext.Entity<TrMessage>()
                                    .Include(e=>e.MessageFors).ThenInclude(e=>e.MessageForDepartements)
                                    .Include(e=>e.MessageFors).ThenInclude(e=>e.MessageForPersonals)
                                    .Include(e=>e.MessageFors).ThenInclude(e=>e.MessageForPositions)
                                    .Include(e=>e.MessageFors).ThenInclude(e=>e.MessageForGrades)
                                    .Where(e=> e.MessageCategory.IdSchool == param.IdSchool
                                            && !e.IsUnsend
                                            && e.PublishStartDate.HasValue
                                            && e.PublishEndDate.HasValue
                                            && e.MessageFors.Any()
                                            && ((e.PublishStartDate <= _dateTime.ServerTime.Date 
                                                    && e.PublishEndDate >= _dateTime.ServerTime.Date)
                                                || e.PublishStartDate >= _dateTime.ServerTime.Date
                                                )
                                            )
                                    .ToListAsync(cancellationToken);
                
                if (listMessagge.Any())
                {
                    DateTime startDate = listMessagge.Select(e => Convert.ToDateTime(e.PublishStartDate)).Min();
                    DateTime EndDate = listMessagge.Select(e => Convert.ToDateTime(e.PublishEndDate)).Max();

                    var idAcademicYear = await _dbContext.Entity<MsPeriod>()
                                        .Include(e => e.Grade).ThenInclude(e => e.MsLevel)
                                        .Where(e => e.StartDate <= startDate.Date && e.EndDate.Date >= EndDate.Date
                                            && e.Grade.MsLevel.MsAcademicYear.IdSchool == param.IdSchool)
                                        .Select(e => e.Grade.MsLevel.IdAcademicYear)
                                        .FirstOrDefaultAsync(cancellationToken);

                    if (idAcademicYear == null)
                    {
                        idAcademicYear = await _dbContext.Entity<MsPeriod>()
                                        .Include(e => e.Grade).ThenInclude(e => e.MsLevel)
                                        .Where(e => e.Grade.MsLevel.MsAcademicYear.IdSchool == param.IdSchool)
                                        .OrderBy(e=>e.StartDate)
                                        .Select(e => e.Grade.MsLevel.IdAcademicYear)
                                        .LastOrDefaultAsync(cancellationToken);
                    }

                    GetUserRolePositionRequest paramUserRole = new GetUserRolePositionRequest
                    {
                        IdAcademicYear = idAcademicYear,
                        IdSchool = param.IdSchool,
                        UserRolePositions = new List<GetUserRolePosition>()
                    };

                    var listUser = await _dbContext.Entity<MsUser>().ToListAsync(cancellationToken);

                    foreach (var itemMessage in listMessagge)
                    {
                        var listMessageFors = itemMessage.MessageFors.ToList();

                        foreach (var itemMessageFor in listMessageFors)
                        {
                            UserRolePersonalOptionType option = UserRolePersonalOptionType.None;
                            if (itemMessageFor.Option == MessageForOption.None)
                                option = UserRolePersonalOptionType.None;
                            else if (itemMessageFor.Option == MessageForOption.All)
                                option = UserRolePersonalOptionType.All;
                            else if (itemMessageFor.Option == MessageForOption.Department)
                                option = UserRolePersonalOptionType.Department;
                            else if (itemMessageFor.Option == MessageForOption.Position)
                                option = UserRolePersonalOptionType.Position;
                            else if (itemMessageFor.Option == MessageForOption.Level)
                                option = UserRolePersonalOptionType.Level;
                            else if (itemMessageFor.Option == MessageForOption.Grade)
                                option = UserRolePersonalOptionType.Grade;
                            else if (itemMessageFor.Option == MessageForOption.PersonalUser)
                                option = UserRolePersonalOptionType.Personal;

                            var newUserRolePosition = new GetUserRolePosition
                            {
                                IdUserRolePositions = itemMessage.Id,
                                Role = itemMessageFor.Role,
                                Option = option,
                                Departemens = itemMessageFor.MessageForDepartements.Select(e => e.IdDepartment).Distinct().ToList(),
                                TeacherPositions = itemMessageFor.MessageForPositions.Select(e => e.IdTeacherPosition).Distinct().ToList(),
                                Level = itemMessageFor.MessageForGrades.Where(e => e.IdLevel != null).Select(e => e.IdLevel).Distinct().ToList(),
                                Homeroom = itemMessageFor.MessageForGrades.Where(e=>e.IdHomeroom!=null).Select(e => e.IdHomeroom).Distinct().ToList(),
                                Personal = itemMessageFor.MessageForPersonals.Where(e => e.IdUser != null).Select(e => e.IdUser).Distinct().ToList(),
                            };

                            paramUserRole.UserRolePositions.Add(newUserRolePosition);
                        }
                    }


                    var apiUserRole = await _rolePosition.GetUserRolePosition(paramUserRole);
                    var GetUserRolePosition = apiUserRole.IsSuccess ? apiUserRole.Payload : null;

                    var listIdMessage = GetUserRolePosition.Select(e => e.IdUserRolePositions).Distinct().ToList();

                    var listRecepient = await _dbContext.Entity<TrMessageRecepient>()
                                        .IgnoreQueryFilters()
                                        .Where(e => listIdMessage.Contains(e.IdMessage))
                                        .ToListAsync(cancellationToken);

                    foreach (var itemMessage in listMessagge)
                    {
                        var recepientByIdMessage = GetUserRolePosition
                                                    .Where(e => e.IdUserRolePositions == itemMessage.Id)
                                                    .Select(e => e.IdUser)
                                                    .Distinct().ToList();

                        if (!recepientByIdMessage.Any())
                            continue;

                        var recepient = listRecepient.Where(e => e.IdMessage == itemMessage.Id).ToList();
                        var listIdRecepient = recepient.Select(e => e.IdRecepient).ToList();

                        //non aktifkan recepient
                        var removeRecepient = recepient.Where(e => !recepientByIdMessage.Contains(e.IdRecepient) && e.MessageFolder == MessageFolder.Inbox).ToList();
                        removeRecepient.ForEach(e => e.IsActive = false);
                        _dbContext.Entity<TrMessageRecepient>().UpdateRange(removeRecepient);

                        //aktifkan recepient
                        var UpdateRecepient = recepient
                                                .Where(e => recepientByIdMessage.Contains(e.IdRecepient)
                                                        && e.MessageFolder == MessageFolder.Inbox
                                                        && !e.IsActive)
                                                .ToList();

                        UpdateRecepient.ForEach(e => e.IsActive = true);
                        _dbContext.Entity<TrMessageRecepient>().UpdateRange(UpdateRecepient);

                        //add recepient
                        var addRecepient = recepientByIdMessage.Where(e => !listIdRecepient.Contains(e)).ToList();
                        var addRecepientByUser = listUser.Where(e => addRecepient.Contains(e.Id))
                                                   .Select(e => new TrMessageRecepient
                                                   {
                                                       Id = Guid.NewGuid().ToString(),
                                                       IdMessage = itemMessage.Id,
                                                       IdRecepient = e.Id
                                                   }).ToList();
                        _dbContext.Entity<TrMessageRecepient>().AddRange(addRecepientByUser);
                    }
                    await _dbContext.SaveChangesAsync();
                }
                #endregion

                #region Update Log Message
                var updateLog = await _dbContext.Entity<TrLogQueueMessage>()
                               .Where(e => e.Id == idLog)
                               .FirstOrDefaultAsync(cancellationToken);

                updateLog.IsDone = true;
                updateLog.IsError = false;
                updateLog.IsProcess = false;
                updateLog.EndDate = _dateTime.ServerTime;

                _dbContext.Entity<TrLogQueueMessage>().Update(updateLog);
                await _dbContext.SaveChangesAsync();
                #endregion
            }
            catch (Exception ex)
            {
                #region Update Log Message
                var updateLog = await _dbContext.Entity<TrLogQueueMessage>()
                               .Where(e => e.Id == idLog)
                               .FirstOrDefaultAsync(cancellationToken);

                updateLog.IsDone = false;
                updateLog.IsError = true;
                updateLog.IsProcess = false;
                updateLog.EndDate = _dateTime.ServerTime;
                updateLog.ErrorMessage = ex.Message;

                _dbContext.Entity<TrLogQueueMessage>().Update(updateLog);
                await _dbContext.SaveChangesAsync();
                #endregion
            }
        }
    }
}
