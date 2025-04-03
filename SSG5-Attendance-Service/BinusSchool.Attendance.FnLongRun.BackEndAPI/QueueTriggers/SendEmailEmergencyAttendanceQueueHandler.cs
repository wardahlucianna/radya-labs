using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Exceptions;
using BinusSchool.Attendance.FnLongRun.HttpTriggers;

namespace BinusSchool.Attendance.FnLongRun.QueueTriggers
{
    public class SendEmailEmergencyAttendanceQueueHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMachineDateTime _dateTime;
        private readonly ILogger<SendEmailEmergencyAttendanceQueueHandler> _logger;
        private readonly SendEmailEmergencyAttendanceHandler _sendEmailEmergencyAttendanceHandler;

        public SendEmailEmergencyAttendanceQueueHandler(
           IAttendanceDbContext dbContext,
           IServiceProvider serviceProvider,
           IMachineDateTime dateTime,
           ILogger<SendEmailEmergencyAttendanceQueueHandler> logger,
           SendEmailEmergencyAttendanceHandler sendEmailEmergencyAttendanceHandler)
        {
            _dbContext = dbContext;
            _serviceProvider = serviceProvider;
            _dateTime = dateTime;
            _logger = logger;
            _sendEmailEmergencyAttendanceHandler = sendEmailEmergencyAttendanceHandler;
        }

        [FunctionName(nameof(SendEmailEmergencyAttendanceQueue))]
        public async Task SendEmailEmergencyAttendanceQueue([QueueTrigger("sendemail-emergency-attendance-queue")] string queueItem, CancellationToken cancellationToken)
        {
            var startExecuteDate = _dateTime.ServerTime;

            var param = JsonConvert.DeserializeObject<QueueAttendanceRequest>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification for IdAttendanceQueue: {0}.", param.IdAttendanceQueue);

            if (string.IsNullOrEmpty(param.IdAttendanceQueue))
                throw new ArgumentNullException(param.IdAttendanceQueue);

            var getQueue = await _dbContext.Entity<TrAttendanceQueue>()
                             .Where(x => x.Id == param.IdAttendanceQueue)
                             .FirstOrDefaultAsync();

            if (getQueue == null)
                throw new BadRequestException(string.Format("TrAttendanceQueue for id: {0} not found", param.IdAttendanceQueue));

            var queueDataTemp = JsonConvert.DeserializeObject<SendEmailEmergencyAttendanceQueue>(getQueue.Data);

            _logger.LogInformation("[Queue] Process send email for : {0}.", queueDataTemp.IdEmergencyReport);

            var GetStudentSafe = await _dbContext.Entity<TrEmergencyAttendance>()
                            .Include(x => x.EmergencyStatus)
                            .Where(x => x.IdEmergencyReport == queueDataTemp.IdEmergencyReport
                            && x.EmergencyStatus.EmergencyStatusName.ToLower() == "safe"
                            && x.SendEmailStatus != true)
                            .ToListAsync();

            if(GetStudentSafe.Count() > 0)
            {
                _logger.LogInformation("[Queue] Process send email to {0} Students with status safe.", GetStudentSafe.Count());
                SendEmailEmergencyAttendanceRequest paramHandler = new SendEmailEmergencyAttendanceRequest();
                paramHandler.studentEmergencyList = GetStudentSafe.Select(a => a.Id).ToList();
                var sendEmailResult = await _sendEmailEmergencyAttendanceHandler.SendEmailEmergencyAttendance(paramHandler);

                if (sendEmailResult != null)
                {
                    getQueue.IsExecuted = true;
                    getQueue.UserUp = "SYS0001";
                    getQueue.StartExecutedDate = startExecuteDate;
                    getQueue.EndExecutedDate = _dateTime.ServerTime;
                    getQueue.Status = sendEmailResult.status;
                    getQueue.Description = sendEmailResult.msg;

                    _dbContext.Entity<TrAttendanceQueue>().Update(getQueue);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("[Queue] Process send email SUCCESS.");
                    _logger.LogInformation(sendEmailResult.msg);
                }
                else
                {
                    getQueue.IsExecuted = true;
                    getQueue.UserUp = "SYS0001";
                    getQueue.StartExecutedDate = startExecuteDate;
                    getQueue.EndExecutedDate = _dateTime.ServerTime;
                    getQueue.Status = false;
                    getQueue.Description = "sendemail return null";
                    _dbContext.Entity<TrAttendanceQueue>().Update(getQueue);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("[Queue] Process send email failed.");
                }

            }
            else
            {
                getQueue.IsExecuted = true;
                getQueue.UserUp = "SYS0001";
                getQueue.StartExecutedDate = startExecuteDate;
                getQueue.EndExecutedDate = _dateTime.ServerTime;
                getQueue.Status = true;
                getQueue.Description = "no data student safe to send email";
                _dbContext.Entity<TrAttendanceQueue>().Update(getQueue);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("[Queue] Process send email [0] Students with status safe.");
            }

        }

    }
}
