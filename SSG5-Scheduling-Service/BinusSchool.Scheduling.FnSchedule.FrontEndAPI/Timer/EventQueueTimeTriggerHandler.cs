using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnSchedule.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Scheduling.FnSchedule.Timer
{
    public class EventQueueTimeTriggerHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IEventSchool _eventService;
        private readonly ILogger<EventQueueTimeTriggerHandler> _logger;

        public EventQueueTimeTriggerHandler(
            IServiceProvider serviceProvider,
            ISchedulingDbContext dbContext,
            ILogger<EventQueueTimeTriggerHandler> logger,
            IEventSchool eventService
            )
        {
            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
            _eventService = eventService;
            _logger = logger;
        }

        [FunctionName(nameof(EventQueueTimeTrigger))]
        public async Task EventQueueTimeTrigger([TimerTrigger(SchedulingTimerConstant.EventQueueConstantTime
#if DEBUG
                //, RunOnStartup = true
#endif
            )]
            TimerInfo myTimer,
            CancellationToken cancellationToken)
        {
            _logger.LogWarning("[EventQueue] Event Run");
            var schools = await _dbContext.Entity<MsSchool>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            _logger.LogWarning($"[EventQueue] Event Start {schools.Count}");

            foreach (var item in schools)
            {
                var apiqueueMessage = await _eventService.QueueEvent(new QueueEventRequest
                {
                    IdSchool = item.Id,
                });
                _logger.LogWarning($"[EventQueue] Event Start School {item.Name}");
                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}
