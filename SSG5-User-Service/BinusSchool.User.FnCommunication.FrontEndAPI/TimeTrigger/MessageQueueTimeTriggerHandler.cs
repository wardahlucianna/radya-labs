using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Api.User.FnCommunication;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.School;
using BinusSchool.User.FnCommunication.TimeTrigger;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BinusSchool.User.FnUser.TimeTrigger
{
    public class MessageQueueTimeTriggerHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserDbContext _dbContext;
        private readonly IMessage _messageService;

        public MessageQueueTimeTriggerHandler(
            IServiceProvider serviceProvider,
            IUserDbContext dbContext,
            IMessage messageService)
        {
            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
            _messageService = messageService;
        }

        [FunctionName(nameof(MessageQueueTimeTrigger))]
        public async Task MessageQueueTimeTrigger([TimerTrigger(CommunicationTimerConstant.MessageQueueConstantTime
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
                var apiqueueMessage = await _messageService.QueueMessages(new QueueMessagesRequest
                {
                    IdSchool = item.Id,
                });
                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}
