using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Api.User.FnCommunication;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.School;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnCommunication.TimeTrigger
{
    public class NotifMessageQueueTimeTriggerHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserDbContext _dbContext;
        private readonly IMessage _messageService;

        public NotifMessageQueueTimeTriggerHandler(
            IServiceProvider serviceProvider,
            IUserDbContext dbContext,
            IMessage messageService)
        {
            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
            _messageService = messageService;
        }

        [FunctionName(nameof(NotifMessageQueueTimeTrigger))]
        public async Task NotifMessageQueueTimeTrigger([TimerTrigger(CommunicationTimerConstant.MessageQueueConstantTime
#if DEBUG
                , RunOnStartup = true
#endif
            )]
            TimerInfo myTimer,
            CancellationToken cancellationToken)
        {
            var schools = await _dbContext.Entity<MsSchool>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var item in schools)
            {
                await _messageService.QueueNotificationMessage(new QueueMessagesRequest
                {
                    IdSchool = item.Id,
                });
                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}
