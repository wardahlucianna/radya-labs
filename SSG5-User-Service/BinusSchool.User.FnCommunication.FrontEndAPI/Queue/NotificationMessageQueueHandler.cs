using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Azure.Storage.Blobs;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Table.Entities;
using BinusSchool.Common.Model;
using System.Threading.Tasks;
using System.Threading;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using BinusSchool.Common.Functions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Entities.School;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Common.Constants;

namespace BinusSchool.User.FnCommunication.Queue
{
    public class NotificationMessageQueueHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationMessageQueueHandler> _logger;
        private readonly IUserDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private CancellationToken cancellationToken;
        public NotificationMessageQueueHandler(IServiceProvider serviceProvider, ILogger<NotificationMessageQueueHandler> logger, IUserDbContext dbContext, IMachineDateTime dateTime)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        [FunctionName(nameof(NotificationMessage))]
        public async Task NotificationMessage([QueueTrigger("notification-message")] string queueItem, CancellationToken cancellationToken)
        {
            var param = JsonConvert.DeserializeObject<QueueMessagesRequest>(queueItem);
            _logger.LogInformation("[Queue] Message");

            var idLog = Guid.NewGuid().ToString();

            #region add Log
            var newLog = new TrLogQueueMsgNotif
            {
                Id = idLog,
                IsProcess = true,
                StartDate = _dateTime.ServerTime,
                IdSchool = param.IdSchool
            };

            _dbContext.Entity<TrLogQueueMsgNotif>().Add(newLog);
            await _dbContext.SaveChangesAsync();
            #endregion

            try
            {
                #region 
                await NotifMsg(param);
                #endregion

                #region Update Log Message
                var updateLog = await _dbContext.Entity<TrLogQueueMsgNotif>()
                               .Where(e => e.Id == idLog)
                               .FirstOrDefaultAsync(cancellationToken);

                updateLog.IsDone = true;
                updateLog.IsError = false;
                updateLog.IsProcess = false;
                updateLog.EndDate = _dateTime.ServerTime;

                _dbContext.Entity<TrLogQueueMsgNotif>().Update(updateLog);
                await _dbContext.SaveChangesAsync();
                #endregion
            }
            catch (Exception ex)
            {
                #region Update Log Message
                var updateLog = await _dbContext.Entity<TrLogQueueMsgNotif>()
                               .Where(e => e.Id == idLog)
                               .FirstOrDefaultAsync(cancellationToken);

                updateLog.IsDone = false;
                updateLog.IsError = true;
                updateLog.IsProcess = false;
                updateLog.EndDate = _dateTime.ServerTime;
                updateLog.ErrorMessage = ex.Message;

                _dbContext.Entity<TrLogQueueMsgNotif>().Update(updateLog);
                await _dbContext.SaveChangesAsync();
                #endregion
            }
        }

        public async Task NotifMsg(QueueMessagesRequest param)
        {
            List<string> listIdScenario = new List<string>()
            {
                MessageScenarioConstant.MSS1,
                MessageScenarioConstant.MSS2,
                MessageScenarioConstant.MSS3,
                MessageScenarioConstant.MSS4,
                MessageScenarioConstant.MSS5,
                MessageScenarioConstant.MSS6,
                MessageScenarioConstant.MSS7,
                MessageScenarioConstant.MSS8,
                MessageScenarioConstant.MSS9,
                MessageScenarioConstant.FD1,
                MessageScenarioConstant.FD2,
                MessageScenarioConstant.EHN1,
                MessageScenarioConstant.EHN2,
                MessageScenarioConstant.EHN3,
            };

            var startDate = _dateTime.ServerTime.Date.AddMonths(-6);

            var listNotif = await _dbContext.Entity<TrNotification>()
                            .Include(e => e.NotificationUsers)
                            .IgnoreQueryFilters()
                            .Where(e => e.IdSchool == param.IdSchool
                                        && e.DateIn >= startDate
                                        && listIdScenario.Contains(e.ScenarioNotificationTemplate))
                            .Select(e => new
                            {
                                e.Id,
                                Data = JsonConvert.DeserializeObject<DataNotification>(e.Data),
                                e.ScenarioNotificationTemplate,
                                e.NotificationUsers
                            })
                            .ToListAsync(cancellationToken);

            var listIdMessage = listNotif.Select(e => e.Data.Id).ToList();

            var listMessage = await _dbContext.Entity<TrMessage>()
                                .IgnoreQueryFilters()
                                .Where(e => listIdMessage.Contains(e.Id))
                                .ToListAsync(cancellationToken);

            var listMessageRecepient = await _dbContext.Entity<TrMessageRecepient>()
                                .Where(e => listIdMessage.Contains(e.IdMessage))
                                .ToListAsync(cancellationToken);

            var listMailing = await _dbContext.Entity<MsGroupMailingList>()
                                    .IgnoreQueryFilters()
                                    .Where(e => listIdMessage.Contains(e.Id))
                                    .ToListAsync(cancellationToken);

            var listMailingMamber = await _dbContext.Entity<MsGroupMailingListMember>()
                                    .IgnoreQueryFilters()
                                    .Where(e => listIdMessage.Contains(e.IdGroupMailingList))
                                    .ToListAsync(cancellationToken);

            foreach (var item in listNotif)
            {
                if (item.Data.Id == null)
                    continue;

                //approval message && Feedback
                if (item.ScenarioNotificationTemplate == MessageScenarioConstant.MSS1
                        || item.ScenarioNotificationTemplate == MessageScenarioConstant.MSS3
                        || item.ScenarioNotificationTemplate == MessageScenarioConstant.MSS4
                        || item.ScenarioNotificationTemplate == MessageScenarioConstant.FD1
                        || item.ScenarioNotificationTemplate == MessageScenarioConstant.FD2)
                {
                    var messageById = listMessage.Where(e => e.Id == item.Data.Id).FirstOrDefault();

                    if (messageById.IsActive)
                    {
                        var listUpdateActive = item.NotificationUsers.Where(e => e.IsDeleteBySystem).ToList();
                        listUpdateActive.ForEach(e => e.IsDeleteBySystem = false);
                        _dbContext.Entity<TrNotificationUser>().UpdateRange(listUpdateActive);
                    }

                    if (!messageById.IsActive)
                    {
                        var listUpdateActive = item.NotificationUsers.Where(e => !e.IsDeleteBySystem).ToList();
                        listUpdateActive.ForEach(e => e.IsDeleteBySystem = true);
                        _dbContext.Entity<TrNotificationUser>().UpdateRange(listUpdateActive);
                    }
                }

                //message recepient
                if (item.ScenarioNotificationTemplate == MessageScenarioConstant.MSS2
                        || item.ScenarioNotificationTemplate == MessageScenarioConstant.MSS7
                        || item.ScenarioNotificationTemplate == MessageScenarioConstant.MSS9)
                {
                    var messageById = listMessage.Where(e => e.Id == item.Data.Id).FirstOrDefault();
                    var listMessageRecepientById = listMessageRecepient.Where(e => e.IdMessage == item.Data.Id && e.IdRecepient != e.UserIn).Select(e => e.IdRecepient).Distinct().ToList();

                    if (messageById.IsActive)
                    {
                        var listUpdateActive = item.NotificationUsers.Where(e => e.IsDeleteBySystem && listMessageRecepientById.Contains(e.IdUser)).ToList();
                        listUpdateActive.ForEach(e => e.IsDeleteBySystem = false);
                        _dbContext.Entity<TrNotificationUser>().UpdateRange(listUpdateActive);

                        var listUpdateNonActive = item.NotificationUsers.Where(e => !e.IsDeleteBySystem && !listMessageRecepientById.Contains(e.IdUser)).ToList();
                        listUpdateNonActive.ForEach(e => e.IsDeleteBySystem = true);
                        _dbContext.Entity<TrNotificationUser>().UpdateRange(listUpdateNonActive);

                        var listAddActive = item.NotificationUsers.Where(e => !listMessageRecepientById.Contains(e.IdUser)).ToList();
                        if (listAddActive.Any())
                        {
                            foreach (var itemAddActive in listAddActive)
                            {
                                TrNotificationUser newNotifUser = new TrNotificationUser
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdUser = itemAddActive.IdUser,
                                    IdNotification = item.Id,
                                    IsDeleteBySystem = false,
                                };
                                _dbContext.Entity<TrNotificationUser>().Add(newNotifUser);
                            }
                        }
                    }

                    if (!messageById.IsActive)
                    {
                        var listUpdateActive = item.NotificationUsers.Where(e => !e.IsDeleteBySystem).ToList();
                        listUpdateActive.ForEach(e => e.IsDeleteBySystem = true);
                        _dbContext.Entity<TrNotificationUser>().UpdateRange(listUpdateActive);
                    }
                }

                //create message
                if (item.ScenarioNotificationTemplate == MessageScenarioConstant.MSS6
                        || item.ScenarioNotificationTemplate == MessageScenarioConstant.MSS5
                        || item.ScenarioNotificationTemplate == MessageScenarioConstant.MSS8)
                {
                    var messageById = listMessage.Where(e => e.Id == item.Data.Id).FirstOrDefault();
                    var listMessageRecepientById = listMessageRecepient.Where(e => e.IdMessage == item.Data.Id).Select(e => e.UserIn).Distinct().ToList();

                    if (messageById.IsActive)
                    {
                        var listUpdateActive = item.NotificationUsers.Where(e => e.IsDeleteBySystem && listMessageRecepientById.Contains(e.IdUser)).ToList();
                        listUpdateActive.ForEach(e => e.IsDeleteBySystem = false);
                        _dbContext.Entity<TrNotificationUser>().UpdateRange(listUpdateActive);

                        var listUpdateNonActive = item.NotificationUsers.Where(e => !e.IsDeleteBySystem && !listMessageRecepientById.Contains(e.IdUser)).ToList();
                        listUpdateNonActive.ForEach(e => e.IsDeleteBySystem = true);
                        _dbContext.Entity<TrNotificationUser>().UpdateRange(listUpdateNonActive);

                        var listAddActive = item.NotificationUsers.Where(e => !listMessageRecepientById.Contains(e.IdUser)).ToList();
                        if (listAddActive.Any())
                        {
                            foreach (var itemAddActive in listAddActive)
                            {
                                TrNotificationUser newNotifUser = new TrNotificationUser
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdUser = itemAddActive.IdUser,
                                    IdNotification = item.Id,
                                    IsDeleteBySystem = false,
                                };
                                _dbContext.Entity<TrNotificationUser>().Add(newNotifUser);
                            }
                        }
                    }

                    if (!messageById.IsActive)
                    {
                        var listUpdateActive = item.NotificationUsers.Where(e => !e.IsDeleteBySystem).ToList();
                        listUpdateActive.ForEach(e => e.IsDeleteBySystem = true);
                        _dbContext.Entity<TrNotificationUser>().UpdateRange(listUpdateActive);
                    }
                }

                //mailing list
                if (item.ScenarioNotificationTemplate == MessageScenarioConstant.EHN1
                        || item.ScenarioNotificationTemplate == MessageScenarioConstant.EHN2
                        || item.ScenarioNotificationTemplate == MessageScenarioConstant.EHN3)
                {
                    var mailingById = listMailing.Where(e => e.Id == item.Data.Id).FirstOrDefault();

                    if (mailingById.IsActive)
                    {
                        var listUpdateActive = item.NotificationUsers.Where(e => e.IsDeleteBySystem).ToList();
                        listUpdateActive.ForEach(e => e.IsDeleteBySystem = false);
                        _dbContext.Entity<TrNotificationUser>().UpdateRange(listUpdateActive);
                    }

                    if (!mailingById.IsActive)
                    {
                        var listUpdateActive = item.NotificationUsers.Where(e => !e.IsDeleteBySystem).ToList();
                        listUpdateActive.ForEach(e => e.IsDeleteBySystem = true);
                        _dbContext.Entity<TrNotificationUser>().UpdateRange(listUpdateActive);
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }

}
public class Department
{
    public int DeptId { get; set; }
    public string DepartmentName { get; set; }
}
public class DataNotification
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Action { get; set; }
    public string MgType { get; set; }
}
